using ToDoCalendarControl.Services;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

namespace ToDoCalendar.MauiHybrid.Platforms.Android;

public class KeyboardService : IKeyboardService
{
    public event Action<bool>? KeyboardStateChanged;

    public KeyboardService()
    {
        var rootView = Platform.CurrentActivity?.Window?.DecorView.FindViewById(global::Android.Resource.Id.Content);

        if (rootView != null)
        {
            var listener = new KeyboardListener(() => rootView.Height, isOpen => KeyboardStateChanged?.Invoke(isOpen));
            rootView.ViewTreeObserver?.AddOnGlobalLayoutListener(listener);
        }
    }

    private class KeyboardListener : Java.Lang.Object, global::Android.Views.ViewTreeObserver.IOnGlobalLayoutListener
    {
        private readonly Func<int> _getViewHeight;
        private readonly Action<bool> _stateChanged;

        private int _previousHeight;

        public KeyboardListener(Func<int> getViewHeight, Action<bool> stateChanged)
        {
            _getViewHeight = getViewHeight;
            _stateChanged = stateChanged;
            _previousHeight = _getViewHeight();
        }

        public void OnGlobalLayout()
        {
            var height = _getViewHeight();
            int heightDiff = _previousHeight - height;
            bool isKeyboardOpen = heightDiff > 200; // Threshold to detect keyboard

            _stateChanged(isKeyboardOpen);
            _previousHeight = height;
        }
    }
}
