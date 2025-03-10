using System;
using System.Windows;

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
