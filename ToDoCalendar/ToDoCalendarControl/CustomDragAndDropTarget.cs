using MetroStyleApps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    class CustomDragAndDropTarget : DragAndDropTarget
    {
        public DateTime Day { get; set; }
        public Controller Controller { get; set; }
    }
}
