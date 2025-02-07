﻿using Plugin.Maui.CalendarStore;
using ToDoCalendarControl.Services;

namespace ToDoCalendar.MauiHybrid.Services;

public class CalendarService : ICalendarService
{
    private const string CalendarName = "ToDoCalendar";

    private readonly ICalendarStore _calendarStore = CalendarStore.Default;

    private Dictionary<string, Calendar>? _calendars;

    public async Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        var events = await ExecuteOnMainThread(() => _calendarStore.GetEvents(null, new DateTimeOffset(startDate), new DateTimeOffset(endDate)));
        var calendars = await GetCalendars();

        return events.Select(x =>
        {
            var calendarEvent = new DeviceEvent
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                DateTime = x.StartDate.DateTime
            };

            if (calendars.TryGetValue(x.CalendarId, out var calendar))
            {
                var color = calendar.Color;
                calendarEvent.SetCalendarColor(color.Alpha, color.Red, color.Green, color.Blue);
            }
            return calendarEvent;
        });
    }

    public async Task<string> CreateCalendarEvent(DeviceEvent calendarEvent)
    {
        var calendars = await GetCalendars();

        return await ExecuteOnMainThread(async () =>
        {
            var calendar = calendars.Values.FirstOrDefault(x => x.Name == CalendarName);
            var color = calendar?.Color ?? Colors.Purple;
            var calendarId = calendar?.Id ?? await _calendarStore.CreateCalendar(CalendarName, color);

            calendarEvent.SetCalendarColor(color.Alpha, color.Red, color.Green, color.Blue);

            var eventId = await _calendarStore.CreateEvent(
                calendarId,
                calendarEvent.Title,
                "",
                "",
                new DateTimeOffset(calendarEvent.DateTime),
                new DateTimeOffset(calendarEvent.DateTime),
                isAllDay: true);

            return eventId;
        });
    }

    public async Task UpdateCalendarEvent(DeviceEvent calendarEvent)
    {
        await ExecuteOnMainThread(async () =>
        {
            var existingEvent = await _calendarStore.GetEvent(calendarEvent.Id);
            var dateOffset = new DateTimeOffset(DateTime.SpecifyKind(calendarEvent.DateTime, DateTimeKind.Utc));

            await _calendarStore.UpdateEvent(
                calendarEvent.Id,
                calendarEvent.Title,
                !string.IsNullOrEmpty(calendarEvent.Description) ? calendarEvent.Description : existingEvent.Description,
                existingEvent.Location,
                dateOffset,
                existingEvent.IsAllDay ? dateOffset : existingEvent.EndDate,
                existingEvent.IsAllDay);
        });
    }

    public async Task DeleteCalendarEvent(string eventId)
    {
        await ExecuteOnMainThread(() => _calendarStore.DeleteEvent(eventId));
    }

    internal async Task<Dictionary<string, Calendar>> GetCalendars()
    {
        if (_calendars == null)
        {
            var calendars = await ExecuteOnMainThread(_calendarStore.GetCalendars);
            _calendars = calendars.ToDictionary(x => x.Id);
        }
        return _calendars;
    }

    private static Task<TResult> ExecuteOnMainThread<TResult>(Func<Task<TResult>> action)
    {
        var tcs = new TaskCompletionSource<TResult>();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                var result = await action();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    private static Task ExecuteOnMainThread(Func<Task> action)
    {
        var tcs = new TaskCompletionSource();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }
}
