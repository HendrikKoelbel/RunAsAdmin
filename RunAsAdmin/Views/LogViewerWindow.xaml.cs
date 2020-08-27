using LiteDB;
using MahApps.Metro.Controls;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RunAsAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für LogViewerWindow.xaml
    /// </summary>
    public partial class LogViewerWindow : MetroWindow
    {

        // TODO: Finish the LogViewer and Update to v2.0.
        public LogViewerWindow()
        {
            InitializeComponent();
        }

        #region Logfile names and paths
        private static List<string> GetAllLogFileNames()
        {
            var list = new List<string>();
            try
            {
                if (Directory.Exists(GlobalVars.PublicDocumentsWithAssemblyName))
                {
                    var fileNames = Directory.GetFiles(GlobalVars.PublicDocumentsWithAssemblyName, "Logger*.db").Select(Path.GetFileName).ToList();
                    foreach (var fileName in fileNames)
                    {
                        list.Add(fileName);
                    }
                    return list;
                }
                throw new DirectoryNotFoundException();
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
                return null;
            }
        }
        private static List<string> GetAllLogFilePaths()
        {
            var list = new List<string>();
            try
            {
                var filePaths = Directory.GetFiles(GlobalVars.PublicDocumentsWithAssemblyName, "Logger*.db").ToList();
                foreach (var filePath in filePaths)
                {
                    list.Add(filePath);
                }
                return list;
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
                return null;
            }
        }
        #endregion

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            SelectLogFileComboBox.SelectionChanged -= SelectLogFileComboBox_SelectionChanged;
            SelectLogFileComboBox.ItemsSource = GetAllLogFileNames();
            SelectLogFileComboBox.SelectedItem = Path.GetFileName(GlobalVars.LoggerPath);
            SelectLogFileComboBox.SelectionChanged += SelectLogFileComboBox_SelectionChanged;
            LoadLogData();
        }

        private async Task<List<LogModel>> GetLogDataAsync()
        {
            var logModels = new List<LogModel>();
            string conString = $"Filename={GlobalVars.PublicDocumentsWithAssemblyName}\\{SelectLogFileComboBox.SelectedItem};ReadOnly=true";
            var result = await Task.Run(() =>
            {
                using var db = new LiteDatabase(conString);
                var Items = db.GetCollection<LogModel>("log");
                foreach (LogModel Item in Items.FindAll())
                {
                    logModels.Add(Item);
                }
                var sortedLogModels = logModels.OrderByDescending(d => d._t.TimeOfDay).ToList();
                return sortedLogModels;
            });
            return result;
        }

        public async void LoadLogData()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                // Load the LogModel Data
                LoggerDataGridView.ItemsSource = await GetLogDataAsync();
                // All column headers are overwritten with the DisplayName value of the property
                LogModel lm = new LogModel();
                var props = lm.GetType().GetProperties();
                for (int i = 0; i < props.Count(); i++)
                {
                    LoggerDataGridView.Columns[i].Header = props[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void LoggerDataGridView_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            LogModel RowDataContaxt = e.Row.DataContext as LogModel;
            if (RowDataContaxt != null)
            {
                //Verbose - Is a computer logging mode that records more information than the usual logging mode. (Verbose means "using more words than necessary".)
                //Debug   - Information that is diagnostically helpful to people more than just developers(IT, sysadmins, etc.).
                //Info    - Generally useful information to log(service start/ stop, configuration assumptions, etc). Info I want to always have available but usually don't care about under normal circumstances. This is my out-of-the-box config level.
                //Warn    - Anything that can potentially cause application oddities, but for which I am automatically recovering. (Such as switching from a primary to backup server, retrying an operation, missing secondary data, etc.)
                //Error   - Any error which is fatal to the operation, but not the service or application(can't open a required file, missing data, etc.). These errors will force user (administrator, or direct user) intervention. These are usually reserved (in my apps) for incorrect connection strings, missing services, etc.
                //Fatal   - Any error that is forcing a shutdown of the service or application to prevent data loss(or further data loss).I reserve these only for the most heinous errors and situations where there is guaranteed to have been data corruption or loss.
                e.Row.BorderThickness = new Thickness(10, 0, 0, 0);
                switch (Enum.Parse(typeof(LogEventLevel), RowDataContaxt._l))
                {
                    case LogEventLevel.Verbose:
                        e.Row.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        break;
                    case LogEventLevel.Debug:
                        e.Row.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 255));
                        break;
                    case LogEventLevel.Information:
                        e.Row.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                        break;
                    case LogEventLevel.Warning:
                        e.Row.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 0));
                        break;
                    case LogEventLevel.Error:
                        e.Row.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                        break;
                    case LogEventLevel.Fatal:
                        e.Row.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 128, 0));
                        break;
                    default:
                        e.Row.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        break;
                }
            }
        }

        private void DeleteLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectLogFileComboBox.SelectedItem != null && SelectLogFileComboBox.Items.Count > 1 && SelectLogFileComboBox.SelectedItem.ToString() != Path.GetFileName(GlobalVars.LoggerPath))
                {
                    var file = GetAllLogFilePaths().Where(s => s.Contains(SelectLogFileComboBox.SelectedItem.ToString())).FirstOrDefault();
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    SelectLogFileComboBox.SelectedItem = Path.GetFileName(GlobalVars.LoggerPath);
                    SelectLogFileComboBox.ItemsSource = GetAllLogFileNames();
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        private void SelectLogFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadLogData();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogData();
        }

        public class LogModel
        {
            //[DisplayName("Log ID")]
            //public ObjectId _id { get; set; }
            [DisplayName("Date and Time")]
            public DateTime _t { get; set; }
            //[DisplayName("Year")]
            //public int _ty { get; set; }
            //[DisplayName("Month")]
            //public int _tm { get; set; }
            //[DisplayName("Day")]
            //public int _td { get; set; }
            //[DisplayName("Week")]
            //public int _tw { get; set; }
            [DisplayName("Log Message")]
            public string _m { get; set; }
            //[DisplayName("Message")]
            //public string _mt { get; set; }
            [DisplayName("ID")]
            public string _i { get; set; }
            [DisplayName("Log Level")]
            public string _l { get; set; }
            [DisplayName("Error Message")]
            public string _x { get; set; }
        }
    }
}
