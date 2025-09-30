using BMRM.Core.Shared.Models;

namespace BMRM.Core.Features.ReleaseMonitor.Vk;

public interface IVkReleaseTextParserService
{
    public Release? ParseSingleReleaseBlock(string html);
}