﻿using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RunAsAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für SplashScreenWindow.xaml
    /// </summary>
    public partial class SplashScreenWindow : MetroWindow
    {
        CancellationTokenSource Cts;
        public SplashScreenWindow(CancellationTokenSource cancellationTokenSource)
        {
            InitializeComponent();
            Cts = cancellationTokenSource;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Cts.Cancel();
        }
    }
}