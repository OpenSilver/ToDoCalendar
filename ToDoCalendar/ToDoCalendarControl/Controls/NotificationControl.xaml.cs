﻿using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ToDoCalendarControl
{
    public partial class NotificationControl : UserControl
    {
        private readonly Storyboard _storyboard;

        public event EventHandler NotificationCompleted;

        public NotificationControl()
        {
            InitializeComponent();

            _storyboard = (Storyboard)Resources["FadeInFadeOutStoryboard"];
        }

        public string Text
        {
            get { return MainText.Text; }
            set { MainText.Text = value; }
        }

        public void Show()
        {
            _storyboard.Stop();
            _storyboard.Completed -= storyboard_Completed;
            _storyboard.Completed += storyboard_Completed;
            _storyboard.Begin();
        }

        void storyboard_Completed(object sender, EventArgs e)
        {
            if (NotificationCompleted != null)
                NotificationCompleted(this, new EventArgs());
        }
    }
}
