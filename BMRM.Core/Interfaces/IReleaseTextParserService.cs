using BMRM.Core.Models;

namespace BMRM.Core.Interfaces;

public interface IReleaseTextParserService
{
    public Release? ParseSingleReleaseBlock(string html);
}