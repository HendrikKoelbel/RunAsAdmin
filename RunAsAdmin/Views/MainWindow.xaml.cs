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
            GlobalVars.Loggi.Information("Initialize program");
            this.Title += $" - v{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}";
            InitializeUpdater();
            InitializeUserRightInfoLabel();
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
                GlobalVars.Loggi.Error(ex, ex.Message);
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
                // Get executable path, fallback to process path for single-file publish
                string executablePath = GlobalVars.ExecutablePath;
                if (string.IsNullOrEmpty(executablePath))
                {
                    executablePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
                    GlobalVars.Loggi.Debug("Using process path as fallback for startup check: {Path}", executablePath);
                }

                // Validate we have a path before proceeding
                if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
                {
                    GlobalVars.Loggi.Warning("Unable to determine executable path for network drive check");
                    return;
                }

                // Extract root drive path (e.g., "C:\Program Files\App.exe" -> "C:\")
                string driveLetter = Path.GetPathRoot(executablePath);
                if (string.IsNullOrEmpty(driveLetter))
                {
                    GlobalVars.Loggi.Warning("Unable to determine drive letter from path: {Path}", executablePath);
                    return;
                }

                DriveInfo info = new DriveInfo(driveLetter);
                if (info.DriveType == DriveType.Network)
                {
                    await this.ShowMessageAsync("Information at the start", "This program cannot be used from a server path. \nPlease run it from the desktop or a local path!");
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        public void InitializeUserRightInfoLabel()
        {
            try
            {
                // Get assembly location, fallback to process path for single-file publish
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    assemblyLocation = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
                    GlobalVars.Loggi.Debug("Using process path as fallback: {Path}", assemblyLocation);
                }

                string runLevel = "Unknown";
                if (!string.IsNullOrEmpty(assemblyLocation) && File.Exists(assemblyLocation))
                {
                    try
                    {
                        runLevel = UACHelper.UACHelper.GetExpectedRunLevel(assemblyLocation).ToString();
                    }
                    catch (Exception rlEx)
                    {
                        GlobalVars.Loggi.Warning(rlEx, "Could not determine run level for: {Path}", assemblyLocation);
                        runLevel = "Unable to determine";
                    }
                }

                UserRightsInfoLabel.Content = $"Current user: {Environment.UserName + " - " + WindowsIdentity.GetCurrent().Name}" +
                    $"\nDefault Behavior: {runLevel}" +
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
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
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
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        public void InitializeUpdater()
        {
            try
            {
                Task task = Task.Run(async () =>
                   {
                       GlobalVars.Loggi.Information("Start Task and check for new update every 5 minutes");
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
                GlobalVars.Loggi.Error(ex, ex.Message);
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
                GlobalVars.Loggi.Information("Check manually for an update");
                if (result.CanUpdate)
                {
                    GlobalVars.Loggi.Information("Can update: {0}", result.CanUpdate);
                    MessageDialogResult dialogResult = await this.ShowMessageAsync("New update available",
                     $"A new version is available.\nOld version: " +
                     $"{Assembly.GetExecutingAssembly().GetName().Version.Major}." +
                     $"{ Assembly.GetExecutingAssembly().GetName().Version.Minor}." +
                     $"{ Assembly.GetExecutingAssembly().GetName().Version.Build}" +
                     $"\nNew version: {result.LastVersion}\nDo you want to update?", MessageDialogStyle.AffirmativeAndNegative);
                    GlobalVars.Loggi.Information($"Old version: {Assembly.GetExecutingAssembly().GetName().Version}");
                    GlobalVars.Loggi.Information($"New version: {result.LastVersion}");
                    if (dialogResult == MessageDialogResult.Affirmative)
                    {
                        GlobalVars.Loggi.Information("Start update process");
                        UpdateWindow updateWindow = new UpdateWindow();
                        updateWindow.ShowDialog();
                    }
                }
                else
                {
                    GlobalVars.Loggi.Information("No update available");
                    await this.ShowMessageAsync("No update available", "There is currently no update available. Please try again later.", MessageDialogStyle.Affirmative);
                }
            }
            catch (HttpRequestException httpRequestEx)
            {
                GlobalVars.Loggi.Warning(httpRequestEx.Message);
                await this.ShowMessageAsync(httpRequestEx.GetType().Name, httpRequestEx.Message, MessageDialogStyle.Affirmative);
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
                await this.ShowMessageAsync(ex.GetType().Name, ex.Message, MessageDialogStyle.Affirmative);
            }
        }
        #endregion

        #region Settings section
        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
        #endregion

        #region Main Region: Button click events
        private async void RestartWithAdminRightsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Checks if the input is empty
                if (String.IsNullOrWhiteSpace((string)DomainComboBox.Text) || String.IsNullOrWhiteSpace((string)UsernameComboBox.Text) || String.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    await this.ShowMessageAsync("Input Required", "Please enter domain, username, and password.");
                    return;
                }

                // Validate ExecutablePath
                if (string.IsNullOrEmpty(GlobalVars.ExecutablePath))
                {
                    GlobalVars.Loggi.Error("ExecutablePath is null or empty, cannot restart application");
                    await this.ShowMessageAsync("Error", "Unable to determine application executable path. Please restart the application manually.");
                    return;
                }

                // Verify executable exists
                if (!File.Exists(GlobalVars.ExecutablePath))
                {
                    GlobalVars.Loggi.Error("Executable not found at: {Path}", GlobalVars.ExecutablePath);
                    await this.ShowMessageAsync("Error", $"Executable not found: {GlobalVars.ExecutablePath}");
                    return;
                }

                Mouse.OverrideCursor = Cursors.Wait;

                // Optional: Add directory security if BasePath is available
                // This allows the impersonated user to access the application directory
                if (!string.IsNullOrEmpty(GlobalVars.BasePath) && Directory.Exists(GlobalVars.BasePath))
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            var credentials = new UserCredentials(GlobalVars.SettingsHelper.Domain, GlobalVars.SettingsHelper.Username, Core.SecurityHelper.Decrypt(GlobalVars.SettingsHelper.Password));
                            bool hasAccess = false;

                            try
                            {
                                SimpleImpersonation.Impersonation.RunAsUser(credentials, SimpleImpersonation.LogonType.Interactive, () =>
                                {
                                    hasAccess = Core.DirectoryHelper.HasDirectoryRights(GlobalVars.BasePath, System.Security.AccessControl.FileSystemRights.ReadAndExecute, winUser: WindowsIdentity.GetCurrent());
                                });
                            }
                            catch (Exception impEx)
                            {
                                GlobalVars.Loggi.Warning(impEx, "Could not check directory rights via impersonation");
                            }

                            if (!hasAccess)
                            {
                                try
                                {
                                    Core.DirectoryHelper.AddDirectorySecurity(GlobalVars.BasePath, winUserString: String.Format(@"{0}\{1}", GlobalVars.SettingsHelper.Domain, GlobalVars.SettingsHelper.Username));
                                    GlobalVars.Loggi.Information("Added directory security for user to access application");
                                }
                                catch (Exception secEx)
                                {
                                    GlobalVars.Loggi.Warning(secEx, "Could not add directory security (will attempt to start anyway)");
                                }
                            }
                        });
                    }
                    catch (Exception dirSecEx)
                    {
                        GlobalVars.Loggi.Warning(dirSecEx, "Directory security check failed (will attempt to start anyway)");
                    }
                }

                // Start the application with new credentials
                await Task.Run(() =>
                {
                    try
                    {
                        ProcessStartInfo ps = new ProcessStartInfo
                        {
                            FileName = GlobalVars.ExecutablePath,
                            Domain = GlobalVars.SettingsHelper.Domain,
                            UserName = GlobalVars.SettingsHelper.Username,
                            Password = Core.Helper.GetSecureString(Core.SecurityHelper.Decrypt(GlobalVars.SettingsHelper.Password)),
                            LoadUserProfile = true,
                            UseShellExecute = false,
                            CreateNoWindow = false,
                            WindowStyle = ProcessWindowStyle.Normal
                        };

                        GlobalVars.Loggi.Information("Starting application as {Domain}\\{User}", ps.Domain, ps.UserName);

                        using (Process p = new Process { StartInfo = ps })
                        {
                            if (p.Start())
                            {
                                GlobalVars.Loggi.Information("Successfully started new process, exiting current instance");
                                Application.Current.Dispatcher.Invoke(() => Environment.Exit(0));
                            }
                            else
                            {
                                GlobalVars.Loggi.Error("Failed to start process");
                                Application.Current.Dispatcher.Invoke(async () =>
                                {
                                    await this.ShowMessageAsync("Error", "Failed to start the application with the specified credentials.");
                                });
                            }
                        }
                    }
                    catch (Win32Exception win32Ex)
                    {
                        GlobalVars.Loggi.Error(win32Ex, "Win32 error starting process: {Message}", win32Ex.Message);
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            await this.ShowMessageAsync("Error", $"Failed to start application: {win32Ex.Message}\n\nMake sure the credentials are correct and you have permission to run this application.");
                        });
                    }
                    catch (Exception startEx)
                    {
                        GlobalVars.Loggi.Error(startEx, "Error starting process: {Message}", startEx.Message);
                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            await this.ShowMessageAsync("Error", $"Failed to restart application: {startEx.Message}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error in RestartWithAdminRightsButton_Click: {Message}", ex.Message);
                await this.ShowMessageAsync("Error", $"An unexpected error occurred: {ex.Message}");
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
                // Checks if the input is empty
                if (String.IsNullOrWhiteSpace((string)DomainComboBox.Text) || String.IsNullOrWhiteSpace((string)UsernameComboBox.Text) || String.IsNullOrWhiteSpace(PasswordTextBox.Password))
                {
                    await this.ShowMessageAsync("Input Required", "Please enter domain, username, and password.");
                    return;
                }

                ///Mapped drives are not available from an elevated prompt
                ///when UAC is configured to "Prompt for credentials" in Windows
                ///https://support.microsoft.com/en-us/help/3035277/mapped-drives-are-not-available-from-an-elevated-prompt-when-uac-is-co#detail%20to%20configure%20the%20registry%20entry
                ///https://stackoverflow.com/a/25908932/11189474
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Filter = "Application (*.exe)|*.exe|All Files|*.*",
                    Title = "Select the applications you want to start",
                    DereferenceLinks = true,
                    Multiselect = true,
                    CheckFileExists = true
                };

                System.Windows.Forms.DialogResult result = fileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
                {
                    string[] paths = fileDialog.FileNames;
                    int successCount = 0;
                    int failureCount = 0;

                    foreach (var path in paths)
                    {
                        try
                        {
                            FileInfo file = new FileInfo(path);

                            // Remove Zone.Identifier (downloaded from internet mark)
                            if (file.Exists && file.AlternateDataStreamExists("Zone.Identifier"))
                            {
                                try
                                {
                                    bool deletedIdentifier = file.DeleteAlternateDataStream("Zone.Identifier");
                                    if (deletedIdentifier)
                                    {
                                        GlobalVars.Loggi.Debug("Removed Zone.Identifier from: {Path}", path);
                                    }
                                }
                                catch (Exception zoneEx)
                                {
                                    GlobalVars.Loggi.Warning(zoneEx, "Could not remove Zone.Identifier from: {Path}", path);
                                }
                            }

                            // Start the application with admin credentials (no UAC prompt)
                            await Task.Run(() =>
                            {
                                try
                                {
                                    ProcessStartInfo ps = new ProcessStartInfo
                                    {
                                        FileName = path,
                                        Domain = GlobalVars.SettingsHelper.Domain,
                                        UserName = GlobalVars.SettingsHelper.Username,
                                        Password = Core.Helper.GetSecureString(Core.SecurityHelper.Decrypt(GlobalVars.SettingsHelper.Password)),
                                        LoadUserProfile = true,
                                        UseShellExecute = false,
                                        CreateNoWindow = false,
                                        WindowStyle = ProcessWindowStyle.Normal
                                    };

                                    GlobalVars.Loggi.Information("Starting {Program} as {Domain}\\{User}", Path.GetFileName(path), ps.Domain, ps.UserName);

                                    using (Process p = new Process { StartInfo = ps })
                                    {
                                        if (p.Start())
                                        {
                                            GlobalVars.Loggi.Information("Successfully started: {Program}", Path.GetFileName(path));
                                            successCount++;
                                        }
                                        else
                                        {
                                            GlobalVars.Loggi.Error("Failed to start: {Program}", Path.GetFileName(path));
                                            failureCount++;
                                        }
                                    }
                                }
                                catch (Win32Exception win32ex)
                                {
                                    GlobalVars.Loggi.Error(win32ex, "Win32 error starting {Program}: {Message}", Path.GetFileName(path), win32ex.Message);
                                    failureCount++;
                                }
                                catch (Exception startEx)
                                {
                                    GlobalVars.Loggi.Error(startEx, "Error starting {Program}: {Message}", Path.GetFileName(path), startEx.Message);
                                    failureCount++;
                                }
                            });
                        }
                        catch (Exception fileEx)
                        {
                            GlobalVars.Loggi.Error(fileEx, "Error processing file: {Path}", path);
                            failureCount++;
                        }
                    }

                    // Show summary
                    if (successCount > 0 || failureCount > 0)
                    {
                        string message = $"Started: {successCount} program(s)";
                        if (failureCount > 0)
                        {
                            message += $"\nFailed: {failureCount} program(s)";
                        }
                        await this.ShowMessageAsync("Program Start Summary", message);
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error in StartProgramWithAdminRightsButton_Click: {Message}", ex.Message);
                await this.ShowMessageAsync("Error", $"An unexpected error occurred: {ex.Message}");
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
                GlobalVars.Loggi.Error(ex, ex.Message);
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
                GlobalVars.Loggi.Error(ex, ex.Message);
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
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        #endregion

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Cancel the update check task
                UpdateCheckCts?.Cancel();

                // Log the shutdown
                GlobalVars.Loggi.Information("Application is shutting down gracefully");

                // Perform graceful shutdown
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, "Error during application shutdown");
                // Only in case of error, force termination
                Environment.Exit(1);
            }
        }
    }
}