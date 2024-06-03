#if SILVERLIGHT
using System;
using System.Windows;
#elif WINRT
using System;
using Windows.UI.Xaml;
#endif

namespace MetroStyleApps
{
    public class DragAndDropEventArgs : EventArgs
    {
        public readonly FrameworkElement Source;

        public DragAndDropEventArgs(FrameworkElement source)
        {
            Source = source;
        }
    }
}
