using Plugin.Maui.CalendarStore;
using ToDoCalendarControl.Services;

namespace ToDoCalendar.MauiHybrid.Services;

public class CalendarService : ICalendarService
{
    private const string CalendarName = "ToDoCalendar";

    readonly ICalendarStore _calendarStore = CalendarStore.Default;

    public async Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        var events = await ExecuteOnMainThread(() => _calendarStore.GetEvents(null, new DateTimeOffset(startDate), new DateTimeOffset(endDate)));

        return events.Select(x => new DeviceEvent
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            DateTime = x.StartDate.DateTime
        });
    }

    public async Task<string> CreateCalendarEvent(DeviceEvent calendarEvent)
    {
        return await ExecuteOnMainThread(async () =>
        {
            var calendars = await _calendarStore.GetCalendars();
            var calendarId = calendars.FirstOrDefault(x => x.Name == CalendarName)?.Id
                ?? await _calendarStore.CreateCalendar(CalendarName, Colors.Purple);

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
