using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Onova;
using Onova.Services;
using Serilog;
using SimpleImpersonation;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Security.AccessControl;
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
            InitializeFlyoutSettings();
            InitializeDataSource();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                UsernameComboBox.Text = GlobalVars.SettingsHelper.Username;
                DomainComboBox.Text = GlobalVars.SettingsHelper.Domain;
                PasswordTextBox.Password = GlobalVars.SettingsHelper.Password;
            }
            catch
            { }
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
        private void InitializeFlyoutSettings()
        {
            SwitchAccent.SelectionChanged -= SwitchAccent_SelectionChanged;
            SwitchAccent.ItemsSource = Enum.GetValues(typeof(GlobalVars.Accents));
            SwitchAccent.SelectionChanged += SwitchAccent_SelectionChanged;
        }

        public void InitializeDataSource()
        {
            try
            {
                Core.Helper.SetDataSource(DomainComboBox, Core.Helper.GetAllDomains().ToArray());
                Core.Helper.SetDataSource(UsernameComboBox, Core.Helper.GetAllUsers().ToArray());
            }
            catch
            { }
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

        #region Update section
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

        #region Flyout Settings section
        private void SwitchTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
            GlobalVars.SettingsHelper.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
        }

        private void SwitchAccent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, SwitchAccent.SelectedItem.ToString());
            GlobalVars.SettingsHelper.Accent = SwitchAccent.SelectedItem.ToString();
        }

        #endregion

        #region Main Region: Button click events
        private async void RestartWithAdminRightsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Checks if the imput is empty
                if (String.IsNullOrWhiteSpace((string)DomainComboBox.Text) || String.IsNullOrWhiteSpace((string)UsernameComboBox.Text) || String.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    throw new ArgumentNullException();
                }

                await Task.Factory.StartNew(() =>
                {
                    bool hasAccess = false;

                    var credentials = new UserCredentials(GlobalVars.SettingsHelper.Domain, GlobalVars.SettingsHelper.Username, GlobalVars.SettingsHelper.Password);
                    SimpleImpersonation.Impersonation.RunAsUser(credentials, SimpleImpersonation.LogonType.Interactive, () =>
                    {
                        using (WindowsIdentity.GetCurrent().Impersonate())
                        {
                            if (Core.Helper.HasFolderRights(GlobalVars.BasePath, FileSystemRights.FullControl, WindowsIdentity.GetCurrent()))
                            {
                                hasAccess = true;
                            }
                            else
                            {
                                hasAccess = false;
                            }
                        }
                    });

                    if (!hasAccess)
                    {
                        Core.Helper.AddDirectorySecurity(GlobalVars.BasePath, String.Format(@"{0}\{1}", GlobalVars.SettingsHelper.Domain, GlobalVars.SettingsHelper.Username), FileSystemRights.FullControl, AccessControlType.Allow);
                    }
                });

                await Task.Factory.StartNew(() =>
                {
                    Process p = new Process();

                    ProcessStartInfo ps = new ProcessStartInfo();

                    ps.FileName = GlobalVars.ExecutablePath;
                    ps.Domain = GlobalVars.SettingsHelper.Domain;
                    ps.UserName = GlobalVars.SettingsHelper.Username;
                    ps.Password = Core.Helper.GetSecureString(GlobalVars.SettingsHelper.Password);
                    ps.LoadUserProfile = true;
                    ps.CreateNoWindow = true;
                    ps.UseShellExecute = false;

                    p.StartInfo = ps;
                    if (p.Start())
                    {
                        Environment.Exit(0);
                    }
                });
            }
            catch (Exception ex)
            {
                // TODO: Logger implementation
                Console.WriteLine(ex.Message);
            }
        }

        private void StartProgramWithAdminRightsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Checks if the imput is empty
                if (String.IsNullOrWhiteSpace((string)DomainComboBox.Text) || String.IsNullOrWhiteSpace((string)UsernameComboBox.Text) || String.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    throw new ArgumentNullException();
                }
                string path = string.Empty;

                //ConfigureWindowsRegistry();
                //UpdateGroupPolicy();
                ///Mapped drives are not available from an elevated prompt 
                ///when UAC is configured to "Prompt for credentials" in Windows
                ///https://support.microsoft.com/en-us/help/3035277/mapped-drives-are-not-available-from-an-elevated-prompt-when-uac-is-co#detail%20to%20configure%20the%20registry%20entry
                ///https://stackoverflow.com/a/25908932/11189474
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Filter = "Application (*.exe)|*.exe";// "All Files|*.*|Link (*.lnk)|*.lnk"
                fileDialog.Title = "Select a application";
                fileDialog.DereferenceLinks = true;
                fileDialog.Multiselect = true;
                System.Windows.Forms.DialogResult result = fileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
                {

                    path = fileDialog.FileName;

                    Task.Factory.StartNew(() =>
                    {
                        //UACHelper.UACHelper.StartElevated(new ProcessStartInfo(path));

                        Process p = new Process();

                        ProcessStartInfo ps = new ProcessStartInfo();

                        ps.FileName = path;
                        ps.Domain = GlobalVars.SettingsHelper.Domain;
                        ps.UserName = GlobalVars.SettingsHelper.Username;
                        ps.Password = Core.Helper.GetSecureString(GlobalVars.SettingsHelper.Password);
                        ps.LoadUserProfile = true;
                        ps.CreateNoWindow = true;
                        ps.UseShellExecute = false;

                        p.StartInfo = ps;
                        p.Start();
                    });
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                // TODO: Logger implementation
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Imput changed events
        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            GlobalVars.SettingsHelper.Password = PasswordTextBox.Password;
        }

        private void DomainComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GlobalVars.SettingsHelper.Domain = DomainComboBox.SelectedItem.ToString();
        }

        private void UsernameComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GlobalVars.SettingsHelper.Username = UsernameComboBox.SelectedItem.ToString();
        }
        #endregion
        private void ViewLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Log.CloseAndFlush();
            LogViewerWindow logViewerWindow = new LogViewerWindow();
            logViewerWindow.Show();
            GlobalVars.Loggi.Information("Nachdem Viewer offen war");
        }
    }
}