using System.Windows;
using System.Windows.Controls;
#if !OPENSILVER
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
#endif

namespace ToDoCalendar
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
#if !OPENSILVER
            SystemTray.BackgroundColor = Colors.White;
            SystemTray.ForegroundColor = Color.FromArgb(255, 50, 50, 50);
            SystemTray.Opacity = 0;
#endif
        }
    }
}