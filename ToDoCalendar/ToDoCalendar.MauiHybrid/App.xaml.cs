using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace ToDoCalendar.MauiHybrid;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();

        Current?.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
            .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "ToDoCalendar" };
    }
}
