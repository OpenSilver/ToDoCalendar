using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
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

    public Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate)
        => _calendarService.GetCalendarEvents(startDate, endDate);

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
