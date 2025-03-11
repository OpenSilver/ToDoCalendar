using System;

namespace ToDoCalendarControl.Controls
{
    internal class CustomDragAndDropTarget : DragAndDropTarget
    {
        public DateTime Day { get; set; }
        public Controller Controller { get; set; }
    }
}
