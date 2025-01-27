using System;

namespace ToDoCalendarControl.Services
{
    public class DeviceEvent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime DateTime { get; set; }

        public override string ToString() => $"{Id} {Title} ({DateTime})";
    }
}
