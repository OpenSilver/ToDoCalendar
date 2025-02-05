using System;

namespace ToDoCalendarControl
{
    public class EventModel
    {
        public EventModel()
        {
            Title = string.Empty;
            Id = string.Empty;
            EventType = EventType.Normal;
        }

        public string Title { get; set; }
        public string Id { get; set; }
        public bool IsMarkedAsDone { get; set; }
        public EventType EventType { get; set; }
    }
}
