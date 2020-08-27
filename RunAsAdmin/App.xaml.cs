using ControlzEx.Theming;
using RunAsAdmin.Views;
using System;
using System.IO;
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

        private void App_Startup(object sender, StartupEventArgs e)
        {
            //Here if called from XAML, otherwise, this code can be in App() 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; // Example 3 
        }
        #region Catch UnhandledExceptions

        // Example 2 
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            GlobalVars.Loggi.Error(e.Exception, e.Exception.Message);
            MessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }

        // Example 3 
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            GlobalVars.Loggi.Error(exception, exception.Message);
            MessageBox.Show(exception.Message);
            if (e.IsTerminating)
            {
                GlobalVars.Loggi.Fatal(exception, exception.Message);
                MessageBox.Show(exception.Message);
                //Now is a good time to write that critical error file!
            }
        }

        // Example 4 
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            GlobalVars.Loggi.Error(e.Exception, e.Exception.Message);
            MessageBox.Show(e.Exception.Message);
            e.SetObserved();
        }

        // Example 5 
        private void WinFormApplication_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            GlobalVars.Loggi.Error(e.Exception, e.Exception.Message);
            MessageBox.Show(e.Exception.Message);
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Initialize the splash screen and set it as the application main window
            var splashScreen = new SplashScreenWindow();
            this.MainWindow = splashScreen;
            splashScreen.Show();
            splashScreen.SplashScreenInfoLabel.Content = "Loading Application...";
            InitializeIOFolders();
            InitializeStyle();

            //In order to ensure the UI stays responsive, we need to
            //Do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //we need to do the work in batches so that we can report progress
                    for (int i = 1; i <= 100; i++)
                    {

                        //Simulate a part of work being done
                        Thread.Sleep(10);

                        //Because we're not on the UI thread, we need to use the Dispatcher
                        //Associated with the splash screen to update the progress bar
                        splashScreen.SplashScreenProgressBar.Dispatcher.Invoke(() => splashScreen.SplashScreenProgressBar.IsIndeterminate = true);

                    }

                    //Once we're done we need to use the Dispatcher
                    //to create and show the MainWindow
                    this.Dispatcher.Invoke(() =>
                    {
                        //Initialize the main window, set it as the application main window
                        //and close the splash screen
                        var mainWindow = new MainWindow();
                        this.MainWindow = mainWindow;
                        mainWindow.Show();
                        mainWindow.Activate();
                        splashScreen.Close();
                    });
                }
                catch (Exception ex)
                {
                    GlobalVars.Loggi.Error(ex, ex.Message);
                }
            });
        }

        public static void InitializeStyle()
        {
            try
            {
                if (File.Exists(GlobalVars.SettingsPath))
                {
                    if (!string.IsNullOrEmpty(GlobalVars.SettingsHelper.Theme))
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
                    if (!string.IsNullOrEmpty(GlobalVars.SettingsHelper.Accent))
                    {
                        if (Enum.TryParse(GlobalVars.SettingsHelper.Accent, out GlobalVars.Accents AccentResult))
                            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, AccentResult.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        public void InitializeIOFolders()
        {
            try
            {
                foreach (var Path in GlobalVars.ListOfPaths)
                {
                    if (!Directory.Exists(Path))
                    {
                        Directory.CreateDirectory(Path);
                        if (Directory.Exists(GlobalVars.PublicDocumentsWithAssemblyName))
                            GlobalVars.Loggi.Debug("Folder was not found and created:\n{0}", Path);
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