using System.Collections.Generic;

namespace ToDoCalendarControl
{
    public class DayModel
    {
        public List<EventModel> Events { get; } = [];

        // If there are no events, we can remove this day from the model.
        public bool CanBeCleanedUp => Events == null || Events.Count == 0;
    }
}
