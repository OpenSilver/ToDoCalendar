using Foundation;
using ToDoCalendarControl.Services;
using UIKit;

namespace ToDoCalendar.MauiHybrid.Platforms.iOS;

public class KeyboardService : IKeyboardService
{
    public event Action<bool>? KeyboardStateChanged;

    public KeyboardService()
    {
        var notificationCenter = NSNotificationCenter.DefaultCenter;

        notificationCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardWillShow);
        notificationCenter.AddObserver(UIKeyboard.DidHideNotification, OnKeyboardWillHide);
    }

    private void OnKeyboardWillShow(NSNotification notification)
    {
        KeyboardStateChanged?.Invoke(true);
    }

    private void OnKeyboardWillHide(NSNotification notification)
    {
        KeyboardStateChanged?.Invoke(false);
    }
}
