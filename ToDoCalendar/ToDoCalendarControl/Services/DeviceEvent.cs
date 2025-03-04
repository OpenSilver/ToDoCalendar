using System;
using System.Windows.Media;
using ToDoCalendarControl.Helpers;

namespace ToDoCalendarControl.Services;

public class DeviceEvent
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }
    public Color CalendarColor { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsAllDay { get; set; }

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

    public void SetCalendarColor(float a, float r, float g, float b)
        => CalendarColor = Color.FromArgb(FloatToByte(a), FloatToByte(r), FloatToByte(g), FloatToByte(b));

    private static byte FloatToByte(float value) => (byte)Math.Round(value * 255);

    public EventModel ToEventModel()
    {
        var model = YamlSerializer.Deserialize(Description) ?? new EventModel();
        model.Id = Id;
        model.Title = Title;
        model.CalendarColor = ColorHelper.AdjustCalendarColor(CalendarColor);
        model.IsReadOnly = IsReadOnly;
        model.DateTime = DateTime;
        model.IsAllDay = IsAllDay;

        // we don't support editing existing events yet
        if (IsRecurring || model.EventType == EventType.Unspecified)
        {
            model.IsReadOnly = true;
        }

        return model;
    }

    public override string ToString() => $"{Id} {Title} ({DateTime})";
}
