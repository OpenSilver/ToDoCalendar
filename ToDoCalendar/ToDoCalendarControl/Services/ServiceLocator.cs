using System;

namespace ToDoCalendarControl.Services;

public static class ServiceLocator
{
    public static IServiceProvider Provider { get; private set; }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        Provider = serviceProvider;
    }
}
