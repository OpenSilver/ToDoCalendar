﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#if !OPENSILVER
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
#endif

namespace ToDoCalendar
{
    public partial class MainPage : Page
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += MainPage_Loaded;
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