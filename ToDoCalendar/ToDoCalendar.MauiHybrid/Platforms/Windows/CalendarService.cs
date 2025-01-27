using ToDoCalendarControl.Services;
using Windows.ApplicationModel.Appointments;

namespace ToDoCalendar.MauiHybrid.Platforms.Windows;

public class CalendarService : ICalendarService
{
    public async IAsyncEnumerable<DeviceEvent> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        var store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

        var appointments = await store.FindAppointmentsAsync(
            startDate,
            endDate - startDate
        );

        foreach (var appointment in appointments)
        {
            yield return new DeviceEvent
            {
                Id = appointment.LocalId,
                Title = appointment.Subject,
                DateTime = appointment.StartTime.DateTime
            };
        }
    }
}
