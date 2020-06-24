using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Onova;
using Onova.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RunAsAdmin.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeUpdater();
            InitializeUserRightsInfoLabel();
            InitializeSettings();

            try
            {
                if (File.Exists(GlobalVars.SettingsPath))
                {
                    if (!String.IsNullOrEmpty(GlobalVars.Settings.Theme))
                    {
                        Enum.TryParse(GlobalVars.Settings.Theme, out GlobalVars.Themes tryParseResult);
                        switch (tryParseResult)
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
                    if (!String.IsNullOrEmpty(GlobalVars.Settings.Accent))
                    {
                        ThemeManager.Current.ChangeThemeColorScheme(Application.Current, GlobalVars.Settings.Accent);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        #region Initialize
        public void InitializeUserRightsInfoLabel()
        {
            UserRightsInfoLabel.Content = String.Format("Current user: {0} " +
                   "\nDefault Behavior: {1} " +
                   "\nIs Elevated: {2}" +
                   "\nIs Administrator: {3}" +
                   "\nIs Desktop Owner: {4}" +
                   "\nProcess Owner: {5}" +
                   "\nDesktop Owner: {6}",
                   Environment.UserName + " - " + WindowsIdentity.GetCurrent().Name,
                   UACHelper.UACHelper.GetExpectedRunLevel(Assembly.GetExecutingAssembly().Location).ToString(),
                   UACHelper.UACHelper.IsElevated.ToString(),
                   UACHelper.UACHelper.IsAdministrator.ToString(),
                   UACHelper.UACHelper.IsDesktopOwner.ToString(),
                   WindowsIdentity.GetCurrent().Name ?? "SYSTEM",
                   UACHelper.UACHelper.DesktopOwner.ToString());
        }
        private void InitializeSettings()
        {
            SwitchAccent.SelectionChanged -= SwitchAccent_SelectionChanged;
            SwitchAccent.ItemsSource = Enum.GetValues(typeof(GlobalVars.Accents));
            SwitchAccent.SelectionChanged += SwitchAccent_SelectionChanged;
        }

        public void InitializeUpdater()
        {
            Task task = Task.Run(async () =>
            {
                while (true)
                {
                    using (var manager = new UpdateManager(new GithubPackageResolver(GlobalVars.GitHubUsername, GlobalVars.GitHubProjectName, GlobalVars.GitHubAssetName), new ZipPackageExtractor()))
                    {
                        var result = await manager.CheckForUpdatesAsync();
                        if (result.CanUpdate)
                        {
                            UpdateBadge.Invoke(new Action(() =>
                            {
                                if (this.UpdateBadge.Badge == null)
                                {
                                    this.UpdateBadge.Badge = new PackIconMaterial() { Kind = PackIconMaterialKind.Update };
                                }
                            }));
                        }
                        else
                        {
                            UpdateBadge.Invoke(new Action(() =>
                            {
                                this.UpdateBadge.Badge = null;
                            }));
                        }
                    }
                    await Task.Delay(300000, UpdateCheckCts.Token);
                }
            }, UpdateCheckCts.Token);
        }
        #endregion

        #region Update Section
        readonly CancellationTokenSource UpdateCheckCts = new CancellationTokenSource();
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Configure to look for packages in specified directory and treat them as zips
                using (var manager = new UpdateManager(new GithubPackageResolver(GlobalVars.GitHubUsername, GlobalVars.GitHubProjectName, GlobalVars.GitHubAssetName), new ZipPackageExtractor()))
                {
                    //Check for updates
                    var result = await manager.CheckForUpdatesAsync();
                    if (result.CanUpdate)
                    {
                        MessageDialogResult dialogResult = await this.ShowMessageAsync("New update available",
                         String.Format("A new version is available.\nOld version: {0}\nNew version: {1}\nDo you want to update?",
                         Assembly.GetExecutingAssembly().GetName().Version,
                         result.LastVersion)
                         , MessageDialogStyle.AffirmativeAndNegative);
                        if (dialogResult == MessageDialogResult.Affirmative)
                        {
                            UpdateWindow updateWindow = new UpdateWindow();
                            updateWindow.ShowDialog();
                        }
                    }
                    else
                    {
                        await this.ShowMessageAsync("No update available", "There is currently no update available. Please try again later.", MessageDialogStyle.Affirmative);
                    }
                }
            }
            catch (HttpRequestException httpRequestEx)
            {
                await this.ShowMessageAsync(httpRequestEx.GetType().Name, httpRequestEx.Message, MessageDialogStyle.Affirmative);
            }
            catch (Exception Ex)
            {
                await this.ShowMessageAsync(Ex.GetType().Name, Ex.Message, MessageDialogStyle.Affirmative);
            }
        }
        #endregion

        private void SwitchTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
            GlobalVars.Settings.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
        }

        private void SwitchAccent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, SwitchAccent.SelectedItem.ToString());
            GlobalVars.Settings.Accent = SwitchAccent.SelectedItem.ToString();
        }
    }
}