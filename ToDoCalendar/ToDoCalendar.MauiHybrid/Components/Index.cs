using Microsoft.AspNetCore.Components;
using OpenSilver.MauiHybrid.Runner;
using System.Windows.Controls.Primitives;

namespace ToDoCalendar.MauiHybrid.Components
{
    [Route("/")]
    public class Index : ComponentBase
    {
        [Inject]
        private IMauiHybridRunner? Runner { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ArgumentNullException.ThrowIfNull(Runner);

            var app = await Runner.RunApplicationAsync<ToDoCalendar.App>();

            if (Application.Current?.MainPage is MainPage mainPage)
            {
                app.MainWindow?.InvokeOnLayoutUpdated(mainPage.Initialize);
            }
        }
    }
}