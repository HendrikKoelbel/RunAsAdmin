using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Onova;
using Onova.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RunAsAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MetroWindow
    {
        #region Private variables
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource UpdateCts = cancellationTokenSource;
        private readonly UpdateManager Manager;
        #endregion

        #region Initialize UpdateWindow
        public UpdateWindow()
        {
            InitializeComponent();
            this.LabelPercentage.Content = "0%/100%";
            Manager = new UpdateManager(new GithubPackageResolver(GlobalVars.GitHubUsername, GlobalVars.GitHubProjectName, GlobalVars.GitHubAssetName), new ZipPackageExtractor());
        }
        public UpdateWindow(UpdateManager updateManager)
        {
            InitializeComponent();
            this.LabelPercentage.Content = "0%/100%";
            Manager = updateManager;
        }
        #endregion

        #region Run update
        private async void UpdateWindow_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                using (var manager = Manager)
                {
                    var updatesResult = await manager.CheckForUpdatesAsync(UpdateCts.Token);
                    Progress<double> progressIndicator = new Progress<double>(ReportProgress);

                    // Prepare an update by downloading and extracting the package
                    // (supports progress reporting and cancellation)
                    await manager.PrepareUpdateAsync(updatesResult.LastVersion, progressIndicator, UpdateCts.Token);

                    // Launch an executable that will apply the update
                    // (can be instructed to restart the application afterwards)
                    manager.LaunchUpdater(updatesResult.LastVersion);

                    // Terminate the running application so that the updater can overwrite files
                    Environment.Exit(0);
                }
            }
            catch (TaskCanceledException)
            {
                if (UpdateCts.IsCancellationRequested)
                {
                    this.Close();
                }
            }
        }
        #endregion

        #region Progress reporter
        public void ReportProgress(double value)
        {
            //Update the UI to reflect the progress value that is passed back.
            this.ProgressBarUpdate.Value = value * 100;
            this.LabelPercentage.Content = String.Format("{0}%/100%", Convert.ToInt32(value * 100));
        }
        #endregion

        #region Button click event with cancellation
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateCts.Cancel();
        }
        #endregion
    }
}