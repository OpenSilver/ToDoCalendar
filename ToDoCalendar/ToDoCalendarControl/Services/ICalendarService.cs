using System;
using System.Collections.Generic;

namespace ToDoCalendarControl.Services
{
    public interface ICalendarService
    {
        IAsyncEnumerable<DeviceEvent> GetCalendarEvents(DateTime startDate, DateTime endDate);
    }
}
