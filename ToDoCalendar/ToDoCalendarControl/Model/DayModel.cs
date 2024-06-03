using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    public class DayModel
    {
        public DayModel()
        {
            Events = new List<EventModel>();
        }

        public List<EventModel> Events { get; set; }

        public bool CanBeCleanedUp
        {
            get
            {
                // If there are no events, we can remove this day from the model.
                return Events == null || Events.Count == 0;
            }
        }
    }
}
