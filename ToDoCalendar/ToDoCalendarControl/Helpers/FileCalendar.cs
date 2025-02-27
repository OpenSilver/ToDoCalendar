using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ToDoCalendarControl.Services;

namespace ToDoCalendarControl.Helpers;

public class FileCalendar : ICalendarService
{
    private const string EventsFileName = "Events.xml";

    private List<DeviceEvent> _allEvents;

    public async Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        if (_allEvents == null)
        {
            var xmlEvents = FileSystemHelpers.ReadTextFromFile(EventsFileName);

            try
            {
                _allEvents = SerializationHelpers.Deserialize<List<DeviceEvent>>(xmlEvents);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        var events = _allEvents?.Where(x => x.DateTime >= startDate && x.DateTime <= endDate) ?? [];
        return await Task.FromResult(events);
    }

    public async Task<string> CreateCalendarEvent(DeviceEvent calendarEvent)
    {
        _allEvents ??= [];

        calendarEvent.Id = Guid.NewGuid().ToString();
        calendarEvent.CalendarColor = Colors.Purple;

        _allEvents.Add(calendarEvent);

        SaveEvents();
        return await Task.FromResult(calendarEvent.Id);
    }

    public async Task UpdateCalendarEvent(DeviceEvent calendarEvent)
    {
        if (_allEvents?.Find(x => x.Id == calendarEvent.Id) is DeviceEvent deviceEvent)
        {
            deviceEvent.Title = calendarEvent.Title;
            deviceEvent.Description = calendarEvent.Description;
            deviceEvent.DateTime = calendarEvent.DateTime;
            deviceEvent.CalendarColor = calendarEvent.CalendarColor;
            deviceEvent.IsReadOnly = calendarEvent.IsReadOnly;
        }

        SaveEvents();
        await Task.CompletedTask;
    }

    public async Task DeleteCalendarEvent(string eventId)
    {
        if (_allEvents?.Find(x => x.Id == eventId) is DeviceEvent deviceEvent)
        {
            _allEvents.Remove(deviceEvent);
        }

        SaveEvents();
        await Task.CompletedTask;
    }

    private void SaveEvents()
    {
        var xmlEvents = SerializationHelpers.Serialize(_allEvents);
        FileSystemHelpers.WriteTextToFile(EventsFileName, xmlEvents);
    }
}
