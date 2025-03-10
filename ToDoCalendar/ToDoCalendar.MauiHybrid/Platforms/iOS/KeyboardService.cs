using Foundation;
using ToDoCalendarControl.Services;
using UIKit;

namespace ToDoCalendar.MauiHybrid.Platforms.iOS;

public class KeyboardService : IKeyboardService
{
    public bool IsKeyboardVisible { get; private set; }

    public event Action<bool>? KeyboardStateChanged;

    public KeyboardService()
    {
        var notificationCenter = NSNotificationCenter.DefaultCenter;

        notificationCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardShow);
        notificationCenter.AddObserver(UIKeyboard.DidHideNotification, OnKeyboardHide);
    }

    private void OnKeyboardShow(NSNotification notification)
        => OnKeyboardStateChanged(true);

    private void OnKeyboardHide(NSNotification notification)
        => OnKeyboardStateChanged(false);

    private void OnKeyboardStateChanged(bool isVisible)
    {
        IsKeyboardVisible = isVisible;
        KeyboardStateChanged?.Invoke(IsKeyboardVisible);
    }
}
