using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using System;
using System.Net.Http;
using System.Reflection;
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
        }


        #region Update Section
        readonly CancellationTokenSource UpdateCheckCts = new CancellationTokenSource();
        public void InitializeUpdater()
        {
            //Task task = Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        using (var manager = new UpdateManager(new GithubPackageResolver(GlobalUpdater.githubUser, GlobalUpdater.githubProject, GlobalUpdater.githubAssetName), new ZipPackageExtractor()))
            //        {
            //            var result = await manager.CheckForUpdatesAsync();
            //            if (result.CanUpdate)
            //            {
            //                UpdateBadge.Invoke(new Action(() =>
            //                {
            //                    if (this.UpdateBadge.Badge == null)
            //                    {
            //                        this.UpdateBadge.Badge = new PackIconMaterial() { Kind = PackIconMaterialKind.Update };
            //                    }
            //                }));
            //            }
            //            else
            //            {
            //                UpdateBadge.Invoke(new Action(() =>
            //                {
            //                    this.UpdateBadge.Badge = null;
            //                }));
            //            }
            //        }
            //        await Task.Delay(300000, UpdateCheckCts.Token);
            //    }
            //}, UpdateCheckCts.Token);
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //     Configure to look for packages in specified directory and treat them as zips
            //    using (var manager = new UpdateManager(new GithubPackageResolver(GlobalUpdater.githubUser, GlobalUpdater.githubProject, GlobalUpdater.githubAssetName), new ZipPackageExtractor()))
            //    {
            //         Check for updates
            //        var result = await manager.CheckForUpdatesAsync();
            //        if (result.CanUpdate)
            //        {
            //            MessageDialogResult dialogResult = await this.ShowMessageAsync("New update available",
            //             String.Format("A new version is available.\nOld version: {0}\nNew version: {1}\nDo you want to update?",
            //             Assembly.GetExecutingAssembly().GetName().Version,
            //             result.LastVersion)
            //             , MessageDialogStyle.AffirmativeAndNegative);
            //            if (dialogResult == MessageDialogResult.Affirmative)
            //            {
            //                UpdateWindow updateWindow = new UpdateWindow();
            //                updateWindow.ShowDialog();
            //            }
            //        }
            //        else
            //        {
            //            await this.ShowMessageAsync("No update available", "There is currently no update available. Please try again later.", MessageDialogStyle.Affirmative);
            //        }
            //    }
            //}
            //catch (HttpRequestException httpRequestEx)
            //{
            //    await this.ShowMessageAsync(httpRequestEx.GetType().Name, httpRequestEx.Message, MessageDialogStyle.Affirmative);
            //}
            //catch (Exception Ex)
            //{
            //    await this.ShowMessageAsync(Ex.GetType().Name, Ex.Message, MessageDialogStyle.Affirmative);
            //}
        }
        #endregion
    }
}