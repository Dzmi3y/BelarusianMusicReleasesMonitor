namespace BMRM.Core.Shared.Models
{
    public class JobLog
    {
        public int Id { get; set; }
        public string JobId { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
