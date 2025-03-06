using ToDoCalendar.Photino.Runner;
using Photino.NET;
using ToDoCalendarControl.Services;

namespace HelloPhotinoApp
{

    //NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
    // Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Window title declared here for visibility
            string windowTitle = "ToDoCalendar";

            ServiceLocator.Initialize(new WebServiceProvider(), Platform.Simulator);

            // Creating a new PhotinoWindow instance with the fluent API
            var window = new PhotinoWindow()
                .SetTitle(windowTitle)
                .SetIconFile("wwwroot/favicon.ico")
                // Resize to a percentage of the main monitor work area
                .SetUseOsDefaultSize(true)
                // Center window in the middle of the screen
                .Center()
                // Users can resize windows by default.
                // Let's make this one fixed instead.
                .SetResizable(false)
                .SetLogVerbosity(0)
                .Load("wwwroot/index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.

            _ = new PhotinoRunner(window).RunApplicationAsync<ToDoCalendar.App>();

            window.WaitForClose(); // Starts the application event loop
        }
    }
}
