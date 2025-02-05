using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ToDoCalendarControl.Services;

public interface ICalendarService
{
    Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate);

    Task<string> CreateCalendarEvent(DeviceEvent calendarEvent);

    Task UpdateCalendarEvent(DeviceEvent calendarEvent);

    Task DeleteCalendarEvent(string eventId);
}
