using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using OpenSilver.WebAssembly;
using System.Threading.Tasks;
using ToDoCalendarControl.Services;

namespace ToDoCalendar.Browser.Pages
{
    [Route("/")]
    public class Index : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder __builder)
        {
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ServiceLocator.Initialize(new WebServiceProvider(), Platform.Web);
            await Runner.RunApplicationAsync<ToDoCalendar.App>();
        }
    }
}