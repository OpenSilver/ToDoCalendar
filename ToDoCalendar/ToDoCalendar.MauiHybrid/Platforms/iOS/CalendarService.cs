using EventKit;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using ToDoCalendarControl.Services;
using UIKit;

namespace ToDoCalendar.MauiHybrid.Platforms.iOS;

public class CalendarService : ICalendarService
{
    public async IAsyncEnumerable<DeviceEvent> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        var eventStore = new EKEventStore();
        bool accessGranted;

        if (UIDevice.CurrentDevice.CheckSystemVersion(17, 0))
        {
            // Use the new methods for iOS 17+
            var access = await eventStore.RequestFullAccessToEventsAsync();
            accessGranted = access.Item1; // Tuple: (accessGranted, error)
        }
        else
        {
            // Use the older method for iOS < 17
            var access = await eventStore.RequestAccessAsync(EKEntityType.Event);
            accessGranted = access.Item1; // Tuple: (accessGranted, error)
        }

        if (!accessGranted)
        {
            throw new UnauthorizedAccessException("Access to calendar events was denied.");
        }

        // Fetch calendar events
        var calendars = eventStore.GetCalendars(EKEntityType.Event);

        var predicate = eventStore.PredicateForEvents(
            startDate.ToNSDate(),
            endDate.ToNSDate(),
            calendars
        );

        var nativeEvents = eventStore.EventsMatching(predicate);

        foreach (var nativeEvent in nativeEvents)
        {
            yield return new DeviceEvent
            {
                Id = nativeEvent.CalendarItemIdentifier,
                Title = nativeEvent.Title,
                DateTime = nativeEvent.StartDate.ToDateTime()
            };
        }
    }
}
