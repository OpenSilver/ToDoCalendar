using Microsoft.AspNetCore.Components;
using OpenSilver.MauiHybrid.Runner;
using ToDoCalendarControl.Services;

namespace ToDoCalendar.MauiHybrid.Components
{
    [Route("/")]
    public class Index : ComponentBase
    {
        [Inject]
        private IMauiHybridRunner? Runner { get; set; }

        [Inject]
        private ICalendarService? CalendarService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ArgumentNullException.ThrowIfNull(Runner);
            await Runner.RunApplicationAsync(() => new ToDoCalendar.App(CalendarService));
        }
    }
}