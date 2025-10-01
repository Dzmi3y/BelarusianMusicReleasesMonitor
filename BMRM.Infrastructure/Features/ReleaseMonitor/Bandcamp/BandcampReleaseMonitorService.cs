using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BMRM.Core.Features.Http;
using BMRM.Core.Features.ReleaseMonitor.Bandcamp;
using BMRM.Core.Features.ReleaseMonitor.Bandcamp.BandcampResponseModels;
using BMRM.Core.Features.ReleaseMonitor.Vk;
using BMRM.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Text.Json;
using BMRM.Core.Shared.Enums;
using BMRM.Core.Shared.Models;
using BMRM.Infrastructure.Utils;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace BMRM.Infrastructure.Features.ReleaseMonitor.Bandcamp;

public class BandcampReleaseMonitorService : IBandcampReleaseMonitorService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ICacheableHttpClient _cacheableHttpClient;
    private readonly string _url = "https://bandcamp.com/api/discover/1/discover_web";
    private readonly HttpClient _httpClient;

    public BandcampReleaseMonitorService(
        AppDbContext db,
        ICacheableHttpClient cacheableHttpClient,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
        _cacheableHttpClient = cacheableHttpClient;
        _httpClient = httpClient;
    }

    public async Task ParseAndSaveAsync()
    {
        var bandcampSearchRequest = new BandcampSearchRequest
        {
            CategoryId = 0,
            TagNormNames = ["belarus"],
            GeonameId = 0,
            Slice = "new",
            TimeFacetId = null,
            Cursor = "*",
            Size = 24,
            IncludeResultTypes = ["a", "s"]
        };

        var jsonBody = JsonConvert.SerializeObject(bandcampSearchRequest, Formatting.Indented);
        try
        {
            var (stream, options) = await GetBandcampResponse(jsonBody);
            await using var stream1 = stream;
            var bandcampResponse = await JsonSerializer.DeserializeAsync<BandcampResponse>(stream, options);

            await SaveNewReleases(bandcampResponse);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to load or process bandcamp response");
        }
        finally
        {
            Log.Logger.Information("BandcampReleaseMonitorService finished at: {time}", DateTimeOffset.Now);
        }
    }

    private async Task<(Stream stream, JsonSerializerOptions options)> GetBandcampResponse(string jsonBody)
    {
        Stream? stream = null;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _url);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            stream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return (stream, options);
        }
        catch
        {
            if (stream != null) await stream.DisposeAsync();
            throw;
        }
    }

    private async Task SaveNewReleases(BandcampResponse? bandcampResponse)
    {
        var releaseIds = _db.Releases
            .AsNoTracking()
            .Select(r => r.Id)
            .ToHashSet();
        var releases = new List<Release>();

        if (bandcampResponse != null)
        {
            foreach (var track in bandcampResponse.Results)
            {
                var id = ReleaseHasher.GetId(track.BandName, track.Title);
                if (releaseIds.Add(id))
                    releases.Add(new Release
                    {
                        Id = id,
                        Title = track.Title,
                        ReleaseDate = track.ReleaseDate,
                        Artist = track.BandName,
                        City = track.BandLocation,
                        CreatedAt = DateTime.UtcNow,
                        Genres = BandcampGenreMapper.GetGenreById(track.BandGenreId),
                        ProcessingStatus = ProcessingStatus.New
                    });
            }
        }

        if (releases.Count > 0)
        {
            await _db.BulkInsertAsync(releases);
        }
    }
}