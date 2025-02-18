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

    internal void Initialize()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RootGrid.Children.Remove(Loader);

            blazorWebView.Focus(); // required to show keyboard after dragging a new event
        });
    }
}
