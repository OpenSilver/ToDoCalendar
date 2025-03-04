using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ToDoCalendarControl.Services;

namespace ToDoCalendarControl.Helpers;

public class CalendarServiceSaver : ICalendarService
{
    private readonly ICalendarService _calendarService;

    public event Action CalendarModified;

    public CalendarServiceSaver(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    // Define four fixed colors for events
    private readonly List<Color> _eventColors = new()
        {
            Color.FromRgb(1, 131, 191),   
            Color.FromRgb(104, 191, 1),   // Green
            Color.FromRgb(191, 144, 1),   // Blue
            Color.FromRgb(108, 1, 191)  // Orange
        };

    // Predefined list of short random event titles (max 20 characters)
    private readonly List<string> _eventTitles = new()
{
    "Meeting",
    "Lunch with John",
    "Project Deadline",
    "Doctor's Visit",
    "Call with Client",
    "Coffee Break",
    "Workout Session",
    "Dinner Plans",
    "Team Sync-up",
    "Code Review",
    "Grocery Shopping",
    "Flight Booking",
    "Morning Jog",
    "Catch-up with Jane",
    "Yoga Class",
    "Book Reading",
    "Study Session",
    "Gaming Night",
    "Bike Ride",
    "Weekend Trip"
};

    private List<DeviceEvent> _allEvents;
    private Random _random = new Random();

    public Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate)
     //=> _calendarService.GetCalendarEvents(startDate, endDate);
    {
        //_allEvents = [];
        //var currentDate = startDate;

        //while (currentDate <= endDate)
        //{
        //    for (int i = 0; i < 4; i++)
        //    {
        //        _allEvents.Add(new DeviceEvent
        //        {
        //            Title = $"Event {i}",
        //            Id = Guid.NewGuid().ToString(),
        //            DateTime = currentDate,
        //            CalendarColor = Color.FromArgb(255, (byte)(i*40), (byte)(i * 40), (byte)(i * 40))
        //        });
        //    }
        //    currentDate = currentDate.AddDays(1);
        //}
        //return Task.FromResult(_allEvents as IEnumerable<DeviceEvent>);

        _allEvents = [];
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            for (int i = 0; i < 1; i++)
            {
                // Pick a random color from the predefined set
                Color selectedColor = _eventColors[_random.Next(_eventColors.Count)];

                // Pick a random short title
                string randomTitle = _eventTitles[_random.Next(_eventTitles.Count)];

                // Generate random RGB values
                byte r = (byte)_random.Next(256);
                byte g = (byte)_random.Next(256);
                byte b = (byte)_random.Next(256);

                // Create a Color object
                Color randomColor = Color.FromRgb(r, g, b);

                _allEvents.Add(new DeviceEvent
                {
                    Title = _eventTitles[_random.Next(_eventTitles.Count)],
                    Id = Guid.NewGuid().ToString(),
                    DateTime = currentDate,
                    CalendarColor = selectedColor
                });
            }
            currentDate = currentDate.AddDays(_random.Next(0,3));
        }
        return Task.FromResult(_allEvents as IEnumerable<DeviceEvent>);
    }


    public async Task<string> CreateCalendarEvent(DeviceEvent calendarEvent)
    {
        try
        {
            var id = await _calendarService.CreateCalendarEvent(calendarEvent);
            //OnCalendarModified();
            return id;
        }
        catch (Exception ex)
        {
            Log("Cannot create an event", ex.ToString());
        }

        return null;
    }

    public async Task UpdateCalendarEvent(DeviceEvent calendarEvent)
    {
        try
        {
            await _calendarService.UpdateCalendarEvent(calendarEvent);
            OnCalendarModified();
        }
        catch (Exception ex)
        {
            Log("Cannot update the event", ex.ToString());
        }
    }

    public async Task DeleteCalendarEvent(string eventId)
    {
        try
        {
            await _calendarService.DeleteCalendarEvent(eventId);
            OnCalendarModified();
        }
        catch (Exception ex)
        {
            Log("Cannot delete the event", ex.ToString());
        }
    }

    public async Task DeleteCalendarEventSilently(string eventId)
    {
        try
        {
            await _calendarService.DeleteCalendarEvent(eventId);
        }
        catch (Exception ex)
        {
            Log("Cannot delete the event", ex.ToString());
        }
    }

    private void OnCalendarModified() => CalendarModified?.Invoke();

    private void Log(string caption, string message)
    {
        Console.WriteLine($"{caption}: {message}");
        MessageBox.Show(message, caption);
    }
}
