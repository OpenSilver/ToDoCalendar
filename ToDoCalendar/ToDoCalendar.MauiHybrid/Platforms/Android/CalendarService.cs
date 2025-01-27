using Android.Content;
using ToDoCalendarControl.Services;
using static Android.Provider.CalendarContract;

namespace ToDoCalendar.MauiHybrid.Platforms.Android;

public class CalendarService : ICalendarService
{
    public async IAsyncEnumerable<DeviceEvent> GetCalendarEvents(DateTime startDate, DateTime endDate)
    {
        if (await RequestPermissionStatus<Permissions.CalendarRead>() != PermissionStatus.Granted)
        {
            throw new UnauthorizedAccessException("READ_CALENDAR permission denied.");
        }
        if (global::Android.App.Application.Context.ContentResolver is not ContentResolver contentResolver)
        {
            throw new InvalidOperationException("ContentResolver is null.");
        }

        var uri = Instances.ContentUri ?? throw new InvalidOperationException("Instances.ContentUri is null.");
        var uriBuilder = uri.BuildUpon();
        if (uriBuilder != null)
        {
            ContentUris.AppendId(uriBuilder, (long)(startDate.ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds);
            ContentUris.AppendId(uriBuilder, (long)(endDate.ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds);
            uri = uriBuilder.Build() ?? throw new InvalidOperationException("Instances.ContentUri is null.");
        }

        string[] projection =
        [
            Instances.EventId,
            Instances.InterfaceConsts.Title,
            Instances.Begin,
            Instances.End,
        ];

        using var cursor = contentResolver.Query(uri, projection, null, null, null);
        if (cursor != null)
        {
            while (cursor.MoveToNext())
            {
                yield return new DeviceEvent
                {
                    Id = cursor.GetString(cursor.GetColumnIndex(Instances.EventId)),
                    Title = cursor.GetString(cursor.GetColumnIndex(Instances.InterfaceConsts.Title)),
                    DateTime = DateTimeOffset.FromUnixTimeMilliseconds(cursor.GetLong(cursor.GetColumnIndex(Instances.Begin))).DateTime
                };
            }
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
