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
#if TEST
            return _calendarService ??= new TestCalendarService();
#else
            return _calendarService ??= new FileCalendar();
#endif
        }
        return null;
    }
}
