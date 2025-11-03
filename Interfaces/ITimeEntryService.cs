using WorkTimeTracker.Models;
using System.Collections.Generic;

namespace WorkTimeTracker.Interfaces
{
    public interface ITimeEntryService
    {
        IEnumerable<TimeEntry> GetAll();
        TimeEntry Create(TimeEntry entry);
        void Stop(int id);
    }
}