using System;

namespace WorkTimeTracker.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }

        public TimeSpan Duration => (End ?? DateTime.Now) - Start;
    }
}