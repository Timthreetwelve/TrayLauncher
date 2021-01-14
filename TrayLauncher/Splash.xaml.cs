// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace TrayLauncher
{
    public partial class Splash : Window
    {
        private DispatcherTimer timer;

        public Splash()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start a timer
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            // Close the Splash screen window when timer expires
            timer.Stop();
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("+++ Splash window closed");
        }
    }
}
