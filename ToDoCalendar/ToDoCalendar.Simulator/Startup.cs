using OpenSilver.Simulator;
using System;
using ToDoCalendarControl.Services;

namespace ToDoCalendar.Simulator
{
    internal static class Startup
    {
        [STAThread]
        static int Main(string[] args)
        {
            ServiceLocator.Initialize(new WebServiceProvider(), Platform.Simulator);
            return SimulatorLauncher.Start(typeof(App));
        }
    }
}
