using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    public class EventModel
    {
        public EventModel()
        {
            Title = string.Empty;
            EventType = EventType.Normal;
        }

        public string Title { get; set; }
        public bool IsMarkedAsDone { get; set; }
        public EventType EventType { get; set; }
    }
}
