using OpenSilver;
using OpenSilver.Themes.Modern;
using System.Windows;
using ToDoCalendar.Utils;

namespace ToDoCalendar
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Theme = new ModernTheme();
            Startup += App_Startup;
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            await FontLoader.LoadAppFonts();

            var mainPage = new MainPage();
            MainWindow.Content = mainPage;

            // prevent text selection
            Interop.ExecuteJavaScriptVoidAsync("$0.classList.add('opensilver-pointer-captured')", Interop.GetDiv(mainPage));
        }
    }
}