using Foundation;
using ToDoCalendarControl.Services;
using UIKit;

namespace ToDoCalendar.MauiHybrid.Platforms.iOS;

public class KeyboardService : IKeyboardService
{
    public event Action<bool>? KeyboardStateChanged;

    public double KeyboardHeight { get; private set; }

    public KeyboardService()
    {
        var notificationCenter = NSNotificationCenter.DefaultCenter;

        notificationCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardWillShow);
        notificationCenter.AddObserver(UIKeyboard.DidHideNotification, OnKeyboardWillHide);
    }

    private void OnKeyboardWillShow(NSNotification notification)
    {
        var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
        KeyboardHeight = keyboardFrame.Height;
        KeyboardStateChanged?.Invoke(true);
    }

    private void OnKeyboardWillHide(NSNotification notification)
    {
        KeyboardHeight = 0;
        KeyboardStateChanged?.Invoke(false);
    }
}
