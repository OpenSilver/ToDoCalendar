using System;
using System.Collections.Generic;

namespace ToDoCalendarControl
{
    public class Model
    {
        public Dictionary<DateTime, DayModel> Days { get; } = [];
    }
}
