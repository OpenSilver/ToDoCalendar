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
                // Generate random RGB values
                byte r = (byte)_random.Next(256);
                byte g = (byte)_random.Next(256);
                byte b = (byte)_random.Next(256);

                // Create a Color object
                Color randomColor = Color.FromRgb(r, g, b);

                _allEvents.Add(new DeviceEvent
                {
                    Title = $"Sample event from the calendar",
                    Id = Guid.NewGuid().ToString(),
                    DateTime = currentDate,
                    CalendarColor = randomColor
                });
            }
            currentDate = currentDate.AddDays(_random.Next(0,10));
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
