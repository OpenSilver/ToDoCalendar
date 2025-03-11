using System;
using System.Windows;

namespace ToDoCalendarControl.Controls
{
    public class DragAndDropEventArgs : EventArgs
    {
        public FrameworkElement Source { get; }

        public DragAndDropEventArgs(FrameworkElement source)
        {
            Source = source;
        }
    }
}
