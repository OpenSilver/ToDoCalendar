using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ToDoCalendarControl
{
    public class Model
    {
        public Model()
        {
            Days = new Dictionary<DateTime, DayModel>();
        }

        public Dictionary<DateTime, DayModel> Days { get; set; }
    }
}
