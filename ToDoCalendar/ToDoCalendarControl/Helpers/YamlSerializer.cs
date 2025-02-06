using System;
using System.Text;

namespace ToDoCalendarControl.Helpers;

public static class YamlSerializer
{
    private const string PrioritySetting = "priority";
    private const string IsDoneSetting = "done";
    private const string DoneValue = "yes";

    public static string Serialize(EventModel model)
    {
        if (model.EventType == EventType.Unspecified)
            return "";

        var sb = new StringBuilder($"{PrioritySetting}: {model.EventType}");
        if (model.IsMarkedAsDone)
        {
            sb.Append($"{Environment.NewLine}{IsDoneSetting}: {DoneValue}");
        }

        return sb.ToString();
    }

    public static EventModel Deserialize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var lines = text.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

        var model = new EventModel();
        foreach (var line in lines)
        {
            var parts = line.Split([": "], StringSplitOptions.None);
            if (parts.Length < 2)
                continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            if (key == PrioritySetting && Enum.TryParse(value, out EventType priority))
                model.EventType = priority;
            else if (key == IsDoneSetting)
                model.IsMarkedAsDone = value.ToLower() == DoneValue;
        }

        return model.EventType != EventType.Unspecified ? model : null;
    }
}
