using ControlzEx.Theming;
using RunAsAdmin.Views;
using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RunAsAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += new StartupEventHandler(App_Startup); // Can be called from XAML 

            DispatcherUnhandledException += App_DispatcherUnhandledException; //Example 2 

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; //Example 4 

            System.Windows.Forms.Application.ThreadException += WinFormApplication_ThreadException; //Example 5 
        }
        void App_Startup(object sender, StartupEventArgs e)
        {
            //Here if called from XAML, otherwise, this code can be in App() 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; // Example 3 
        }
        #region Catch UnhandledExceptions

        // Example 2 
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }

        // Example 3 
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            MessageBox.Show(exception.Message);
            if (e.IsTerminating)
            {
                //Now is a good time to write that critical error file!
            }
        }

        // Example 4 
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            e.SetObserved();
        }

        // Example 5 
        void WinFormApplication_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
        #endregion


        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public readonly CancellationTokenSource Cts = cancellationTokenSource;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Initialize the splash screen and set it as the application main window
            var splashScreen = new SplashScreenWindow(Cts);
            this.MainWindow = splashScreen;
            splashScreen.Show();

            //In order to ensure the UI stays responsive, we need to
            //Do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ////we need to do the work in batches so that we can report progress
                    //for (int i = 1; i <= 100; i++)
                    //{
                    if (Cts.IsCancellationRequested)
                        throw new TaskCanceledException();

                    Thread.Sleep(50);

                    splashScreen.SplashScreenInfoLabel.Dispatcher.Invoke(() => splashScreen.SplashScreenInfoLabel.Content = "Loading Style...");
                    InitializeStyle();

                    Thread.Sleep(50);

                    ////Simulate a part of work being done
                    //Thread.Sleep(10);

                    ////Because we're not on the UI thread, we need to use the Dispatcher
                    ////Associated with the splash screen to update the progress bar
                    //splashScreen.SplashScreenProgressBar.Dispatcher.Invoke(() => splashScreen.SplashScreenProgressBar.IsIndeterminate = true);

                    //}

                    //Once we're done we need to use the Dispatcher
                    //to create and show the MainWindow
                    this.Dispatcher.Invoke(() =>
                    {
                        //Initialize the main window, set it as the application main window
                        //and close the splash screen
                        var mainWindow = new MainWindow();
                        this.MainWindow = mainWindow;
                        mainWindow.Show();
                        splashScreen.Close();
                    });
                }
                catch (TaskCanceledException)
                {
                    //Terminates the program if the task is cancelled
                    Environment.Exit(0);
                }
            }, Cts.Token);
        }

        public static void InitializeStyle()
        {
            try
            {
                // TODO: Check for file rights
                if (File.Exists(GlobalVars.SettingsPath))
                {
                    if (!String.IsNullOrEmpty(GlobalVars.SettingsHelper.Theme))
                    {
                        if (Enum.TryParse(GlobalVars.SettingsHelper.Theme, out GlobalVars.Themes ThemesResult))
                            switch (ThemesResult)
                            {
                                case GlobalVars.Themes.Dark:
                                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, ThemeManager.BaseColorDark);
                                    break;
                                case GlobalVars.Themes.Light:
                                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, ThemeManager.BaseColorLight);
                                    break;
                                default:
                                    break;
                            }
                    }
                    if (!String.IsNullOrEmpty(GlobalVars.SettingsHelper.Accent))
                    {
                        if (Enum.TryParse(GlobalVars.SettingsHelper.Accent, out GlobalVars.Accents AccentResult))
                            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, GlobalVars.SettingsHelper.Accent);
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
    }
}