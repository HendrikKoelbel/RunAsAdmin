using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Onova;
using Onova.Services;
using SimpleImpersonation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Trinet.Core.IO.Ntfs;

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
            //GlobalVars.Loggi.Information("Initialize program");
            this.Title += $" - v{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}";
            InitializeUpdater();
            InitializeUserRightInfoLabel();
            InitializeFlyoutSettings();
            InitializeDataSource();
        }

        #region Windowevents
        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                InitializeStartUpDetails();
                UsernameComboBox.Text = GlobalVars.SettingsHelper.Username ?? string.Empty;
                DomainComboBox.Text = GlobalVars.SettingsHelper.Domain ?? string.Empty;
                PasswordTextBox.Password = Core.SecurityHelper.Decrypt(GlobalVars.SettingsHelper.Password) ?? string.Empty;
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        private void ViewLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LogViewerWindow logViewerWindow = new LogViewerWindow();
            logViewerWindow.Show();
        }
        #endregion

        #region Initialize
        public async void InitializeStartUpDetails()
        {
            try
            {
                DriveInfo info = new DriveInfo(GlobalVars.ExecutablePath);
                if (info.DriveType == DriveType.Network)
                    await this.ShowMessageAsync("Information at the start", "This program cannot be used from a server path. \nPlease run it from the desktop or a local path!");
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        public void InitializeUserRightInfoLabel()
        {
            try
            {
                UserRightsInfoLabel.Content = $"Current user: {Environment.UserName + " - " + WindowsIdentity.GetCurrent().Name}" +
                    $"\nDefault Behavior: {UACHelper.UACHelper.GetExpectedRunLevel(Assembly.GetExecutingAssembly().Location)}" +
                    $"\nIs Elevated: {UACHelper.UACHelper.IsElevated}" +
                    $"\nIs Administrator: {UACHelper.UACHelper.IsAdministrator}" +
                    $"\nIs Desktop Owner: {UACHelper.UACHelper.IsDesktopOwner}" +
                    $"\nProcess Owner: {WindowsIdentity.GetCurrent().Name ?? "SYSTEM"}" +
                    $"\nDesktop Owner: {UACHelper.UACHelper.DesktopOwner}";

                if (!UACHelper.UACHelper.IsAdministrator)
                {
                    StartProgramWithAdminRightsButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        private void InitializeFlyoutSettings()
        {
            SwitchThemeToggle.Toggled -= SwitchThemeToggle_Toggled;
            SwitchThemeToggle.IsOn = true ? GlobalVars.SettingsHelper.Theme == ThemeManager.BaseColorDark : false;
            SwitchThemeToggle.Toggled += SwitchThemeToggle_Toggled;
            SwitchAccentComboBox.SelectionChanged -= SwitchAccentComboBox_SelectionChanged;
            SwitchAccentComboBox.ItemsSource = Enum.GetValues(typeof(GlobalVars.Accents));
            SwitchAccentComboBox.SelectedIndex = SwitchAccentComboBox.Items.IndexOf((GlobalVars.Accents)Enum.Parse(typeof(GlobalVars.Accents), GlobalVars.SettingsHelper.Accent));
            SwitchAccentComboBox.SelectionChanged += SwitchAccentComboBox_SelectionChanged;
        }

        public void InitializeDataSource()
        {
            try
            {
                Core.Helper.SetDataSource(DomainComboBox, Core.Helper.GetAllDomains().ToArray());
                Core.Helper.SetDataSource(UsernameComboBox, Core.Helper.GetAllUsers().ToArray());
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        public void InitializeUpdater()
        {
            try
            {
                Task task = Task.Run(async () =>
                   {
                       //GlobalVars.Loggi.Information("Start Task and check for new update every 5 minutes");
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
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        #endregion

        #region Update section
        private readonly CancellationTokenSource UpdateCheckCts = new CancellationTokenSource();
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Configure to look for packages in specified directory and treat them as zips
                using var manager = new UpdateManager(new GithubPackageResolver(GlobalVars.GitHubUsername, GlobalVars.GitHubProjectName, GlobalVars.GitHubAssetName), new ZipPackageExtractor());
                //Check for updates
                var result = await manager.CheckForUpdatesAsync();
                //GlobalVars.Loggi.Information("Check manually for an update");
                if (result.CanUpdate)
                {
                    //GlobalVars.Loggi.Information("Can update: {0}", result.CanUpdate);
                    MessageDialogResult dialogResult = await this.ShowMessageAsync("New update available",
                     $"A new version is available.\nOld version: " +
                     $"{Assembly.GetExecutingAssembly().GetName().Version.Major}." +
                     $"{ Assembly.GetExecutingAssembly().GetName().Version.Minor}." +
                     $"{ Assembly.GetExecutingAssembly().GetName().Version.Build}" +
                     $"\nNew version: {result.LastVersion}\nDo you want to update?", MessageDialogStyle.AffirmativeAndNegative);
                    //GlobalVars.Loggi.Information($"Old version: {Assembly.GetExecutingAssembly().GetName().Version}");
                    //GlobalVars.Loggi.Information($"New version: {result.LastVersion}");
                    if (dialogResult == MessageDialogResult.Affirmative)
                    {
                        //GlobalVars.Loggi.Information("Start update process");
                        UpdateWindow updateWindow = new UpdateWindow();
                        updateWindow.ShowDialog();
                    }
                }
                else
                {
                    //GlobalVars.Loggi.Information("No update available");
                    await this.ShowMessageAsync("No update available", "There is currently no update available. Please try again later.", MessageDialogStyle.Affirmative);
                }
            }
            catch (HttpRequestException httpRequestEx)
            {
                //GlobalVars.Loggi.Warning(httpRequestEx.Message);
                await this.ShowMessageAsync(httpRequestEx.GetType().Name, httpRequestEx.Message, MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
                await this.ShowMessageAsync(ex.GetType().Name, ex.Message, MessageDialogStyle.Affirmative);
            }
        }
        #endregion

        #region Flyout Settings section
        private void SwitchThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SwitchThemeToggle.IsOn == true)
                {
                    // Switch Theme
                    ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
                    // Display current theme on the SwitchLabel
                    SwitchThemeToggle.OnContent = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Save current theme in the settings
                    GlobalVars.SettingsHelper.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Log this event
                    //GlobalVars.Loggi.Information($"Theme was changed from {ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)).BaseColorScheme} to {ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme}");
                }
                else
                {
                    // Switch Theme
                    ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
                    // Display current theme on the SwitchLabel
                    SwitchThemeToggle.OffContent = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Save current theme in the settings
                    GlobalVars.SettingsHelper.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Log this event
                    //GlobalVars.Loggi.Information($"Theme was changed from {ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)).BaseColorScheme} to {ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme}");
                }
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        private void SwitchThemeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
                GlobalVars.SettingsHelper.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                //GlobalVars.Loggi.Information($"Theme was changed from {ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)).BaseColorScheme} to {ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme}");
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        private void SwitchAccentComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, SwitchAccentComboBox.SelectedItem.ToString());
                GlobalVars.SettingsHelper.Accent = SwitchAccentComboBox.SelectedItem.ToString();
                //GlobalVars.Loggi.Information($"Accent was changed to {SwitchAccentComboBox.SelectedItem.ToString()}");
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
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
                Mouse.OverrideCursor = Cursors.Wait;

                bool hasAccess = false;
                await Task.Factory.StartNew(() =>
                {
                    var credentials = new UserCredentials(GlobalVars.SettingsHelper.Domain, GlobalVars.SettingsHelper.Username, Core.SecurityHelper.Decrypt(GlobalVars.SettingsHelper.Password));
                    SimpleImpersonation.Impersonation.RunAsUser(credentials, SimpleImpersonation.LogonType.Interactive, () =>
                    {
                        using (WindowsIdentity.GetCurrent().Impersonate())
                        {
                            hasAccess = Core.DirectoryHelper.HasDirectoryRights(GlobalVars.BasePath, System.Security.AccessControl.FileSystemRights.FullControl, winUser: WindowsIdentity.GetCurrent());
                        }
                    });
                    if (!hasAccess)
                    {
                        Core.DirectoryHelper.AddDirectorySecurity(GlobalVars.BasePath, winUserString: String.Format(@"{0}\{1}", GlobalVars.SettingsHelper.Domain, GlobalVars.SettingsHelper.Username));
                    }
                });

                await Task.Factory.StartNew(() =>
                {
                    Process p = new Process();

                    ProcessStartInfo ps = new ProcessStartInfo
                    {
                        FileName = GlobalVars.ExecutablePath,
                        Domain = GlobalVars.SettingsHelper.Domain,
                        UserName = GlobalVars.SettingsHelper.Username,
                        Password = Core.Helper.GetSecureString(Core.SecurityHelper.Decrypt(GlobalVars.SettingsHelper.Password)),
                        LoadUserProfile = true,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    p.StartInfo = ps;
                    if (p.Start())
                    {
                        Environment.Exit(0);
                    }
                });
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void StartProgramWithAdminRightsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Checks if the imput is empty
                if (String.IsNullOrWhiteSpace((string)DomainComboBox.Text) || String.IsNullOrWhiteSpace((string)UsernameComboBox.Text) || String.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    throw new ArgumentNullException();
                }

                ///Mapped drives are not available from an elevated prompt 
                ///when UAC is configured to "Prompt for credentials" in Windows
                ///https://support.microsoft.com/en-us/help/3035277/mapped-drives-are-not-available-from-an-elevated-prompt-when-uac-is-co#detail%20to%20configure%20the%20registry%20entry
                ///https://stackoverflow.com/a/25908932/11189474
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Filter = "Application (*.exe)|*.exe|All Files|*.*",// "All Files|*.*|Link (*.lnk)|*.lnk"
                    Title = "Select the applications you want to start",
                    DereferenceLinks = true,
                    Multiselect = true
                };
                System.Windows.Forms.DialogResult result = fileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
                {

                    string[] paths = fileDialog.FileNames;
                    FileInfo file;
                    foreach (var path in paths)
                    {
                        file = new FileInfo(path);
                        if (file.Exists && file.AlternateDataStreamExists("Zone.Identifier"))
                        {
                            bool deletedIdentifier = file.DeleteAlternateDataStream("Zone.Identifier");
                            if (deletedIdentifier == false)
                                return;
                        }
                        await Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                UACHelper.UACHelper.StartElevated(new ProcessStartInfo(path));
                            }
                            catch (Win32Exception win32ex)
                            {
                                //GlobalVars.Loggi.Error(win32ex, win32ex.Message);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        #endregion

        #region Imput changed events
        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                GlobalVars.SettingsHelper.Password = Core.SecurityHelper.Encrypt(PasswordTextBox.Password);
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        private void DomainComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                GlobalVars.SettingsHelper.Domain = DomainComboBox.SelectedItem.ToString();
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        private void UsernameComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                GlobalVars.SettingsHelper.Username = UsernameComboBox.SelectedItem.ToString();
            }
            catch (Exception ex)
            {
                //GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        #endregion

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
        }
    }
}