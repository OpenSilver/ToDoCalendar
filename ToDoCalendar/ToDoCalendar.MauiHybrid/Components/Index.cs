using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenSilver.MauiHybrid.Runner;
using System.Windows.Controls.Primitives;

namespace ToDoCalendar.MauiHybrid.Components
{
    [Route("/")]
    public class Index : ComponentBase
    {
        [Inject]
        private IMauiHybridRunner? Runner { get; set; }

        [Inject]
        public required IJSRuntime JS { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ArgumentNullException.ThrowIfNull(Runner);

            var app = await Runner.RunApplicationAsync<ToDoCalendar.App>();

#if MACCATALYST // workaround to prevent displaying text too bold on Mac
            await JS.InvokeVoidAsync("eval", "document.body.style.fontSynthesis = 'none'");
#endif

            if (Application.Current?.MainPage is MainPage mainPage)
            {
                app.MainWindow?.InvokeOnLayoutUpdated(mainPage.Initialize);
            }
        }
    }
}