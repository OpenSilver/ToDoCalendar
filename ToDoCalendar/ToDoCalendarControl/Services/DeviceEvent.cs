using System;
using ToDoCalendarControl.Helpers;

namespace ToDoCalendarControl.Services;

public class DeviceEvent
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }

    public DeviceEvent()
    {
    }

    public DeviceEvent(EventModel eventModel, DateTime day)
    {
        Id = eventModel.Id;
        Title = eventModel.Title;
        Description = YamlSerializer.Serialize(eventModel);
        DateTime = day;
    }

    public EventModel ToEventModel()
    {
        var model = YamlSerializer.Deserialize(Description) ?? new EventModel();
        model.Id = Id;
        model.Title = Title;

        return model;
    }

    public override string ToString() => $"{Id} {Title} ({DateTime})";
}
