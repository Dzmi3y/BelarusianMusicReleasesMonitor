using BMRM.Core.Shared.Models;

namespace BMRM.Core.Features.ReleaseMonitor;

public interface IReleaseTextParserService
{
    public Release? ParseSingleReleaseBlock(string html);
}