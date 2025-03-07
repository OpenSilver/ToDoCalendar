using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ToDoCalendarControl.Services;

public class TestCalendarService : ICalendarService
{
    private const int GenerateDaysBeforeToday = 300;
    private const int GenerateDaysAfterToday = 300;

    private readonly Color WorkCalendarColor = Color.FromRgb(108, 134, 216); // Blue
    private readonly Color PersonalCalendarColor = Color.FromRgb(104, 191, 1); // Green
    private readonly Color HolidaysCalendarColor = Color.FromRgb(191, 144, 1); // Orange
    private readonly List<DeviceEvent> _events = [];

    public TestCalendarService()
    {
        GenerateEvents();
    }

    private void GenerateEvents()
    {
        var date = DateTime.Today.AddDays(-GenerateDaysBeforeToday);
        for (int i = 0; i <= GenerateDaysBeforeToday + GenerateDaysAfterToday; i++)
        {
            AddWorkRecurringEvents(date);
            AddPersonalRecurringEvents(date);
            AddHolidays(date);

            date = date.AddDays(1);
        }

        AddOneTimeEvents();
        AddToDos();
    }

    private void AddWorkRecurringEvents(DateTime date)
    {
        if (date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
        {
            _events.Add(CreateEvent(date.AddHours(9), "Standup Meeting", WorkCalendarColor));
        }

        if (date.DayOfWeek is DayOfWeek.Tuesday &&
            Math.Abs((date - DateTime.Today).Days % 14) <= 7) // every two weeks
        {
            _events.Add(CreateEvent(date.AddHours(10), "Exec Meeting", WorkCalendarColor));
        }
    }

    private void AddPersonalRecurringEvents(DateTime date)
    {
        if (date.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Thursday)
        {
            _events.Add(CreateEvent(date.AddHours(7), "Gym", PersonalCalendarColor));
        }
    }

    private void AddHolidays(DateTime date)
    {
        if (date.Month == 1 && date.Day == 1)
        {
            _events.Add(CreateEvent(date, "New Year", HolidaysCalendarColor, isAllDay: true));
        }
        if (date.Month == 3 && date.Day == 8)
        {
            _events.Add(CreateEvent(date, "International Women's Day", HolidaysCalendarColor, isAllDay: true));
        }
        if (date.Month == 12 && date.Day == 25)
        {
            _events.Add(CreateEvent(date, "Christmas", HolidaysCalendarColor, isAllDay: true));
        }
    }

    private void AddOneTimeEvents()
    {
        _events.Add(CreateEvent(DateTime.Today.AddDays(13).AddHours(16).AddMinutes(30), "Doctor Appointment", PersonalCalendarColor, false, true));
        _events.Add(CreateEvent(new DateTime(2025, 4, 20), "Easter Day", HolidaysCalendarColor, isRecurring: false));
    }

    private DeviceEvent CreateEvent(DateTime date, string title, Color calendarColor, bool isRecurring = true, bool isAllDay = false) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Title = title,
        Description = "",
        DateTime = date,
        CalendarColor = calendarColor,
        IsReadOnly = true,
        IsRecurring = isRecurring,
        IsAllDay = isAllDay
    };

    private void AddToDos()
    {
        var firstDay = GetWeekDay(DateTime.Today);
        _events.Add(CreateToDo("Buy groceries", firstDay, EventType.Normal));
        _events.Add(CreateToDo("Finish the report", firstDay, EventType.HighPriority));

        var secondDay = GetWeekDay(firstDay.AddDays(1));
        _events.Add(CreateToDo("Prepare for the meeting", secondDay, EventType.HighPriority));
        _events.Add(CreateToDo("Send the documents", secondDay, EventType.LowPriority));
        _events.Add(CreateToDo("Book the tickets", secondDay, EventType.Normal));
        _events.Add(CreateToDo("Read the book", secondDay, EventType.Info));
    }

    private DateTime GetWeekDay(DateTime date)
    {
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }
        return date;
    }

    private DeviceEvent CreateToDo(string title, DateTime date, EventType eventType, bool isDone = false) => new(new EventModel
    {
        Id = Guid.NewGuid().ToString(),
        Title = title,
        EventType = eventType,
        IsMarkedAsDone = isDone
    }, date);

    public Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        return Task.FromResult(_events.Where(e => e.DateTime >= startDate && e.DateTime <= endDate).OrderBy(x => x.DateTime).AsEnumerable());
    }

    public Task<string> CreateCalendarEvent(DeviceEvent calendarEvent)
    {
        calendarEvent.Id = Guid.NewGuid().ToString();
        _events.Add(calendarEvent);
        return Task.FromResult(calendarEvent.Id);
    }

    public Task UpdateCalendarEvent(DeviceEvent calendarEvent)
    {
        if (_events?.Find(x => x.Id == calendarEvent.Id) is DeviceEvent deviceEvent)
        {
            deviceEvent.Title = calendarEvent.Title;
            deviceEvent.Description = calendarEvent.Description;
            deviceEvent.DateTime = calendarEvent.DateTime;
            deviceEvent.CalendarColor = calendarEvent.CalendarColor;
            deviceEvent.IsReadOnly = calendarEvent.IsReadOnly;
        }

        return Task.CompletedTask;
    }

    public Task DeleteCalendarEvent(string eventId)
    {
        if (_events?.Find(x => x.Id == eventId) is DeviceEvent deviceEvent)
        {
            _events.Remove(deviceEvent);
        }

        return Task.CompletedTask;
    }
}
