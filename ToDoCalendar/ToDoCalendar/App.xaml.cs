using OpenSilver;
using System.Windows;
using ToDoCalendar.Utils;

namespace ToDoCalendar
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Startup += App_Startup;
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            await FontLoader.LoadAppFonts();

            var mainPage = new MainPage();
            Window.Current.Content = mainPage;

            // prevent text selection
            Interop.ExecuteJavaScriptVoidAsync("$0.classList.add('opensilver-pointer-captured')", Interop.GetDiv(mainPage));
        }
    }
}