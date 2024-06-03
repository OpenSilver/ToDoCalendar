using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media.Animation;

namespace ToDoCalendarControl
{
    public partial class NotificationControl : UserControl
    {
        public event EventHandler NotificationCompleted;

        public NotificationControl()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return MainText.Text; }
            set { MainText.Text = value; }
        }

        public void Show()
        {
            var storyboard = (Storyboard)this.Resources["FadeInFadeOutStoryboard"];
            storyboard.Completed -= storyboard_Completed;
            storyboard.Completed += storyboard_Completed;
            storyboard.Begin();
        }

        void storyboard_Completed(object sender, EventArgs e)
        {
            if (NotificationCompleted != null)
                NotificationCompleted(this, new EventArgs());
        }
    }
}
