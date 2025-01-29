using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using OpenSilver.MauiHybrid.Runner;
using ToDoCalendarControl.Services;

namespace ToDoCalendar.MauiHybrid;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .ConfigureMauiHandlers(conf =>
            {
#if ANDROID
                conf.AddHandler<BlazorWebView, AndroidWebViewHandler>();
#endif
            });

        builder.Services.AddScoped<IMauiHybridRunner, MauiHybridRunner>();
        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register platform-specific services
#if ANDROID
        builder.Services.AddSingleton<ICalendarService, Platforms.Android.CalendarService>();
#elif IOS
        builder.Services.AddSingleton<ICalendarService, Platforms.iOS.CalendarService>();
#elif MACCATALYST
        builder.Services.AddSingleton<ICalendarService, Platforms.MacCatalyst.CalendarService>();
#elif WINDOWS
        builder.Services.AddSingleton<ICalendarService, Platforms.Windows.CalendarService>();
#endif

        ServiceLocator.Initialize(builder.Services.BuildServiceProvider());

        return builder.Build();
    }
}
