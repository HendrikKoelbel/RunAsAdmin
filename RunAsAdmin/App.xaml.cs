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


                    splashScreen.SplashScreenInfoLabel.Dispatcher.Invoke(() => splashScreen.SplashScreenInfoLabel.Content = "Checking FileAccess...");
                    InitializeFileAccess();

                    Thread.Sleep(50);

                    splashScreen.SplashScreenInfoLabel.Dispatcher.Invoke(() => splashScreen.SplashScreenInfoLabel.Content = "Loading Style...");
                    InitializeStyle();

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

        public static void InitializeFileAccess()
        {
            //try
            //{
            //    if (Directory.Exists(GlobalVars.ProgramDataWithAssemblyName))
            //    {
            //        if (!Core.FileHelper.HasFolderRights(GlobalVars.ProgramDataWithAssemblyName, System.Security.AccessControl.FileSystemRights.FullControl, String.Format(@"{0}\{1}", GlobalVars.SettingsHelper.Domain, GlobalVars.SettingsHelper.Username)))
            //        {
            //            Core.FileHelper.AddDirectorySecurity(GlobalVars.ProgramDataWithAssemblyName, WindowsIdentity.GetCurrent().Name, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    GlobalVars.Loggi.Error(ex, ex.Message);
            //}
        }
    }
}