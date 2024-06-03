using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    public class RefreshDayRequestedEventArgs : EventArgs
    {
        public readonly DateTime Day;
        public readonly bool SetFocusToLastEventOfTheDay;

        public RefreshDayRequestedEventArgs(DateTime day, bool setFocusToLastEventOfTheDay = false)
        {
            Day = day;
            SetFocusToLastEventOfTheDay = setFocusToLastEventOfTheDay;
        }
    }
}
