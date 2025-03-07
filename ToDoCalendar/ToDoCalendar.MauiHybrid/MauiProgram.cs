using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Logging;
using OpenSilver.MauiHybrid.Runner;
using ToDoCalendar.MauiHybrid.Services;
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

#if TEST
        builder.Services.AddSingleton<ICalendarService, TestCalendarService>();
#else
        builder.Services.AddSingleton<ICalendarService, CalendarService>();
#endif

#if ANDROID
        builder.Services.AddSingleton<IKeyboardService, Platforms.Android.KeyboardService>();
#elif IOS
        builder.Services.AddSingleton<IKeyboardService, Platforms.iOS.KeyboardService>();
#endif

        builder.Services.AddSingleton<IDispatcher>(provider =>
        {
            var dispatcher = Application.Current?.Dispatcher;
            return dispatcher ?? throw new InvalidOperationException("Dispatcher is not available.");
        });

        var app = builder.Build();

        ServiceLocator.Initialize(app.Services, GetPlatform());

        return app;
    }

    private static ToDoCalendarControl.Services.Platform GetPlatform()
    {
#if ANDROID
        return ToDoCalendarControl.Services.Platform.Android;
#elif IOS
        return ToDoCalendarControl.Services.Platform.iOS;
#elif MACCATALYST
        return ToDoCalendarControl.Services.Platform.Mac;
#elif WINDOWS
        return ToDoCalendarControl.Services.Platform.Windows;
#else
        return ToDoCalendarControl.Services.Platform.Unknown;
#endif
    }
}
