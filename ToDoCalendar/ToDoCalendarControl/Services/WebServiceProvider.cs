using System;
using ToDoCalendarControl.Helpers;

namespace ToDoCalendarControl.Services;

public class WebServiceProvider : IServiceProvider
{
    private ICalendarService _calendarService;

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(ICalendarService))
        {
            return _calendarService ??= new FileCalendar();
        }
        return null;
    }
}
