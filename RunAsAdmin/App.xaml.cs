using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RunAsAdmin.Views;

namespace RunAsAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //initialize the splash screen and set it as the application main window
            //var splashScreen = new SplashScreenWindow();
            //this.MainWindow = splashScreen;
            //splashScreen.Show();

            //in order to ensure the UI stays responsive, we need to
            //do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(100); // 1000
                ////we need to do the work in batches so that we can report progress
                //for (int i = 1; i <= 100; i++)
                //{
                //    //simulate a part of work being done
                //    System.Threading.Thread.Sleep(30);

                //    //because we're not on the UI thread, we need to use the Dispatcher
                //    //associated with the splash screen to update the progress bar
                //    splashScreen.Dispatcher.Invoke(() => splashScreen.Progress = i);
                //}

                //once we're done we need to use the Dispatcher
                //to create and show the main window
                this.Dispatcher.Invoke(() =>
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    var mainWindow = new MainWindow();
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                    //splashScreen.Close();
                });
            });
        }

    }
}