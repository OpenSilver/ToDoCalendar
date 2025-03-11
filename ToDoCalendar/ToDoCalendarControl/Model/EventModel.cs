using System;
using System.Windows.Media;

namespace ToDoCalendarControl
{
    public class EventModel
    {
        public string Title { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public bool IsMarkedAsDone { get; set; }
        public bool IsReadOnly { get; set; }
        public EventType EventType { get; set; } = EventType.Unspecified;
        public Color? CalendarColor { get; set; }
        public DateTime? TemporaryCreationDate { get; set; } // Not intended to be persisted
        public bool IsDeleted { get; set; }
        public DateTime? DateTime { get; set; }
        public bool IsAllDay { get; set; }
    }
}
