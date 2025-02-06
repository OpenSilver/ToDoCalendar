using Android.Content;
using Android.Provider;
using Microsoft.Maui.Graphics.Platform;
using ToDoCalendarControl.Services;
using static Android.Provider.CalendarContract;

namespace ToDoCalendar.MauiHybrid.Platforms.Android;

public class CalendarService : ICalendarService
{
    private readonly ContentResolver _contentResolver = Platform.AppContext.ApplicationContext?.ContentResolver
        ?? throw new InvalidOperationException("ContentResolver is null");

    private int? _calendarId;

    public async Task<IEnumerable<DeviceEvent>> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        await EnsurePermissionIsGranted<Permissions.CalendarRead>();
        return GetEvents(startDate, endDate);
    }

    private IEnumerable<DeviceEvent> GetEvents(DateTime startDate, DateTime endDate)
    {
        var uri = Instances.ContentUri ?? throw new InvalidOperationException("Instances.ContentUri is null.");
        var uriBuilder = uri.BuildUpon();
        if (uriBuilder != null)
        {
            ContentUris.AppendId(uriBuilder, GetMilliseconds(startDate));
            ContentUris.AppendId(uriBuilder, GetMilliseconds(endDate));
            uri = uriBuilder.Build() ?? throw new InvalidOperationException("Uri is null.");
        }

        string[] projection =
        [
            Instances.EventId,
            Instances.InterfaceConsts.Title,
            Instances.InterfaceConsts.Description,
            Instances.Begin,
        ];
        var cursor = _contentResolver.Query(uri, projection, null, null, null);
        if (cursor != null)
        {
            while (cursor.MoveToNext())
            {
                yield return new DeviceEvent
                {
                    Id = cursor.GetString(cursor.GetColumnIndex(Instances.EventId)),
                    Title = cursor.GetString(cursor.GetColumnIndex(Instances.InterfaceConsts.Title)),
                    Description = cursor.GetString(cursor.GetColumnIndex(Instances.InterfaceConsts.Description)),
                    DateTime = DateTimeOffset.FromUnixTimeMilliseconds(cursor.GetLong(cursor.GetColumnIndex(Instances.Begin))).DateTime
                };
            }
        }
    }

    public async Task<string> CreateCalendarEvent(DeviceEvent deviceEvent)
    {
        await EnsurePermissionIsGranted<Permissions.CalendarWrite>();

        int calendarId = GetPrimaryCalendarId() ?? throw new InvalidOperationException("Not found primary calendar to save.");
        var uri = Events.ContentUri ?? throw new InvalidOperationException("Events.ContentUri is null.");
        var eventValues = new ContentValues();
        eventValues.Put(Events.InterfaceConsts.CalendarId, calendarId);
        eventValues.Put(Events.InterfaceConsts.Title, deviceEvent.Title);
        eventValues.Put(Events.InterfaceConsts.Description, deviceEvent.Description);
        var dateMilliseconds = GetMilliseconds(DateTime.SpecifyKind(deviceEvent.DateTime, DateTimeKind.Utc));
        eventValues.Put(Events.InterfaceConsts.Dtstart, dateMilliseconds);
        eventValues.Put(Events.InterfaceConsts.Dtend, dateMilliseconds);
        eventValues.Put(Events.InterfaceConsts.AllDay, true);
        eventValues.Put(Events.InterfaceConsts.EventTimezone, TimeZoneInfo.Local.Id);

        var resultUri = _contentResolver.Insert(uri, eventValues);

        if (!long.TryParse(resultUri?.LastPathSegment, out var eventId))
        {
            throw new InvalidOperationException("There was an error saving the event.");
        }

        return eventId.ToString();
    }

    public async Task UpdateCalendarEvent(DeviceEvent calendarEvent)
    {
        // await EnsurePermissionIsGranted<Permissions.CalendarWrite>();
        await new Services.CalendarService().UpdateCalendarEvent(calendarEvent);
    }

    public async Task DeleteCalendarEvent(string eventId)
    {
        //await EnsurePermissionIsGranted<Permissions.CalendarWrite>();
        await new Services.CalendarService().DeleteCalendarEvent(eventId);
    }

    private int? GetPrimaryCalendarId()
    {
        if (_calendarId == null)
        {
            string[] projection =
            [
                Calendars.InterfaceConsts.Id,
                Calendars.InterfaceConsts.IsPrimary
            ];

            var uri = Calendars.ContentUri ?? throw new InvalidOperationException("Calendars.ContentUri is null.");
            using var cursor = _contentResolver.Query(uri, projection, null, null, null);

            if (cursor != null)
            {
                while (cursor.MoveToNext())
                {
                    bool isPrimary = cursor.GetInt(cursor.GetColumnIndex(Calendars.InterfaceConsts.IsPrimary)) == 1;
                    if (isPrimary)
                    {
                        _calendarId = cursor.GetInt(cursor.GetColumnIndex(Calendars.InterfaceConsts.Id));
                        break;
                    }
                }
            }

            _calendarId ??= CreateCalendar(uri, "ToDoCalendar", Microsoft.Maui.Graphics.Colors.Purple);
        }

        return _calendarId;
    }

    private int CreateCalendar(global::Android.Net.Uri uri, string name, Color color)
    {
        var calendarValues = new ContentValues();
        calendarValues.Put(Calendars.InterfaceConsts.AccountName, name);
        calendarValues.Put(Calendars.InterfaceConsts.AccountType, AccountTypeLocal);
        calendarValues.Put(Calendars.Name, name);
        calendarValues.Put(Calendars.InterfaceConsts.CalendarDisplayName, name);
        calendarValues.Put(Calendars.InterfaceConsts.CalendarColor, color.AsColor());
        calendarValues.Put(Calendars.InterfaceConsts.CalendarAccessLevel, (int)CalendarAccess.AccessOwner);
        calendarValues.Put(Calendars.InterfaceConsts.OwnerAccount, name);

        var insertCalendarUri = uri.BuildUpon()
           ?.AppendQueryParameter(CallerIsSyncadapter, "true")
           ?.AppendQueryParameter(Calendars.InterfaceConsts.AccountName, name)
           ?.AppendQueryParameter(Calendars.InterfaceConsts.AccountType, AccountTypeLocal)
           ?.Build()
           ?? throw new InvalidOperationException("There was an error saving the calendar.");

        var idUrl = _contentResolver.Insert(insertCalendarUri, calendarValues);

        if (!int.TryParse(idUrl?.LastPathSegment, out var savedId))
        {
            throw new InvalidOperationException("There was an error saving the calendar.");
        }

        return savedId;
    }

    private static long GetMilliseconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }

    private static async Task EnsurePermissionIsGranted<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        if (await RequestPermissionStatus<TPermission>() != PermissionStatus.Granted)
        {
            throw new UnauthorizedAccessException($"{typeof(TPermission)} permission denied.");
        }
    }

    private static async Task<PermissionStatus> RequestPermissionStatus<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        var tcs = new TaskCompletionSource<PermissionStatus>();
        var status = await Permissions.CheckStatusAsync<TPermission>();

        if (status != PermissionStatus.Granted)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                status = await Permissions.RequestAsync<TPermission>();
                tcs.SetResult(status);
            });
        }
        else
        {
            tcs.SetResult(status);
        }

        return await tcs.Task;
    }
}
