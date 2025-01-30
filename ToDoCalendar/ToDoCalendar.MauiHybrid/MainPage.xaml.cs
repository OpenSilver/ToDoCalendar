namespace ToDoCalendar.MauiHybrid;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

#if WINDOWS
        Loader.Background = null;
#endif
    }

    internal void HideLoader()
    {
        MainThread.BeginInvokeOnMainThread(() => RootGrid.Children.Remove(Loader));
    }
}
