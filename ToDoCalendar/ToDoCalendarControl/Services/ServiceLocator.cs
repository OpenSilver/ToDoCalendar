using System;

namespace ToDoCalendarControl.Services;

public static class ServiceLocator
{
    public static IServiceProvider Provider { get; private set; }

    public static Platform Platform { get; private set; }

    public static void Initialize(IServiceProvider serviceProvider, Platform platform)
    {
        Provider = serviceProvider;
        Platform = platform;
    }
}
