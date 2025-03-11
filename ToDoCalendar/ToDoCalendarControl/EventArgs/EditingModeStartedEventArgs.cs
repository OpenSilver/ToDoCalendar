using System;
using System.Windows.Controls;

namespace ToDoCalendarControl
{
    public class EditingModeStartedEventArgs : EventArgs
    {
        public readonly TextBox TextBox;
        public readonly EventModel EventModel;
        public readonly DayModel DayModel;
        public readonly DateTime Day;

        public EditingModeStartedEventArgs(TextBox textBox, EventModel eventModel, DayModel dayModel, DateTime day)
        {
            TextBox = textBox;
            EventModel = eventModel;
            DayModel = dayModel;
            Day = day;
        }
    }
}
