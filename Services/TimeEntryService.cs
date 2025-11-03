using WorkTimeTracker.Models;
using WorkTimeTracker.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace WorkTimeTracker.Services
{
    public class TimeEntryService : ITimeEntryService
    {
        private readonly List<TimeEntry> _entries = new();

        public IEnumerable<TimeEntry> GetAll() => _entries;

        public TimeEntry Create(TimeEntry entry)
        {
            entry.Id = _entries.Count > 0 ? _entries.Max(e => e.Id) + 1 : 1;
            _entries.Add(entry);
            return entry;
        }

        public void Stop(int id)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry != null)
            {
                entry.End = System.DateTime.Now;
            }
        }
    }
}