using System;
using System.Windows.Media;

namespace ToDoCalendarControl
{
    public class EventModel
    {
        public EventModel()
        {
            Title = string.Empty;
            Id = string.Empty;
            EventType = EventType.Unspecified;
        }

        public string Title { get; set; }
        public string Id { get; set; }
        public bool IsMarkedAsDone { get; set; }
        public bool IsReadOnly { get; set; }
        public EventType EventType { get; set; }
        public Color? CalendarColor { get; set; }
        public DateTime? TemporaryCreationDate { get; set; } // Not intended to be persisted
    }
}
