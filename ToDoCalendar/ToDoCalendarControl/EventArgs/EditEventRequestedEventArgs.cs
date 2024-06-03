using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    public class EditEventRequestedEventArgs : EventArgs
    {
        public readonly EventModel EventModel;
        public readonly DayModel DayModel;
        public readonly DateTime Day;

        public EditEventRequestedEventArgs(EventModel eventModel, DayModel dayModel, DateTime day)
        {
            EventModel = eventModel;
            DayModel = dayModel;
            Day = day;
        }
    }
}
