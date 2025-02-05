using System;

namespace ToDoCalendarControl.Services;

public class DeviceEvent
{
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime DateTime { get; set; }

    public DeviceEvent()
    {
    }

    public DeviceEvent(EventModel eventModel, DateTime day)
    {
        Id = eventModel.Id;
        Title = eventModel.Title;
        DateTime = day;
    }

    public override string ToString() => $"{Id} {Title} ({DateTime})";
}
