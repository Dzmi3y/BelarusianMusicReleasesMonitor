namespace BMRM.Core.Shared.Models
{
    public class JobDefinition
    {
        public string JobId { get; set; } = default!;
        public string Cron { get; set; } = default!;
        public bool Enabled { get; set; }
    }

}
