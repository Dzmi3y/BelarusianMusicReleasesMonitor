using BMRM.Core.Shared.Models;

namespace BMRM.Core.Features.ReleaseMonitor;

public interface IVkReleaseTextParserService
{
    public Release? ParseSingleReleaseBlock(string html);
}