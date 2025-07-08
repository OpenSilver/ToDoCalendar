using ToDoCalendarControl.Services;
using Rect = Android.Graphics.Rect;

namespace ToDoCalendar.MauiHybrid.Platforms.Android;

public class KeyboardService : IKeyboardService
{
    private bool _isKeyboardVisible;

    private Action<Rect>? _getWindowVisibleDisplayFrame;
    private Func<int>? _getScreenHeight;

    public event Action<bool>? KeyboardStateChanged;

    public bool IsKeyboardVisible => _isKeyboardVisible;

    public KeyboardService()
    {
        SetupKeyboardListener();
    }

    private void SetupKeyboardListener()
    {
        var activity = Platform.CurrentActivity;
        if (activity?.Window?.DecorView?.RootView?.ViewTreeObserver != null)
        {
            var rootView = activity.Window.DecorView.RootView;
            rootView.ViewTreeObserver.GlobalLayout += OnGlobalLayout;
            _getWindowVisibleDisplayFrame = rootView.GetWindowVisibleDisplayFrame;
            _getScreenHeight = () => rootView.Height;
        }
    }

    private void OnGlobalLayout(object? sender, EventArgs e)
    {
        if (_getWindowVisibleDisplayFrame == null || _getScreenHeight == null) return;

        var rect = new Rect();
        _getWindowVisibleDisplayFrame(rect);

        var screenHeight = _getScreenHeight();
        var keypadHeight = screenHeight - rect.Bottom;

        var isKeyboardVisible = keypadHeight > screenHeight * 0.15;

        if (_isKeyboardVisible != isKeyboardVisible)
        {
            _isKeyboardVisible = isKeyboardVisible;
            KeyboardStateChanged?.Invoke(isKeyboardVisible);
        }
    }
}
