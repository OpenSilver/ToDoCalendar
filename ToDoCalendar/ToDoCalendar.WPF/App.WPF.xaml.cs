using System.Windows;
using System.Windows.Media.Imaging;
using ToDoCalendarControl.Services;

namespace ToDoCalendar
{
    public partial class App : Application
    {
        public App()
        {
            // todo: load font
            InitializeComponent();

            ServiceLocator.Initialize(new WebServiceProvider());
            MainWindow = new Window { Title = "ToDoCalendar", Icon = new BitmapImage(new Uri("appicon.ico", UriKind.Relative)) };
            MainWindow.Show();

            var mainPage = new MainPage();
            MainWindow.Content = mainPage;
        }
    }
}