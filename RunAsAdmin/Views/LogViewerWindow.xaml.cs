using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RunAsAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für LogViewerWindow.xaml
    /// </summary>
    public partial class LogViewerWindow : MetroWindow
    {

        // TODO: Finish the LogViewer and Update to v2.0.0

        public LogViewerWindow()
        {
            InitializeComponent();
        }

        #region Properties
        public object CurrentModel { get; set; } = new SimpleLoggerModel();
        public string CurrentLogPath { get; set; } = GlobalVars.LoggerPath;
        public LoggerType CurrentLoggerTypeEnum { get; set; } = LoggerType.Simple;
        public string CurrentLoggerType { get; set; } = Enum.GetName(typeof(LoggerType), LoggerType.Simple);
        #endregion

        #region Logfile names and paths
        private static List<string> GetAllLogFileNames()
        {
            var list = new List<string>();
            try
            {
                var files = Directory.GetFiles(GlobalVars.PublicDocumentsWithAssemblyName, "Logger*.db").Select(System.IO.Path.GetFileName).ToList();
                foreach (var file in files)
                {
                    list.Add(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        private static List<string> GetAllLogFilePaths()
        {
            var list = new List<string>();
            try
            {
                var files = Directory.GetFiles(GlobalVars.PublicDocumentsWithAssemblyName, "Logger*.db").ToList();
                foreach (var file in files)
                {
                    list.Add(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        #endregion

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            SelectLogFileComboBox.ItemsSource = GetAllLogFileNames();
            SelectLogFileComboBox.SelectionChanged -= SelectLogFileComboBox_SelectionChanged;
            SelectLogFileComboBox.SelectedItem = System.IO.Path.GetFileName(GlobalVars.LoggerPath);
            SelectLogFileComboBox.SelectionChanged += SelectLogFileComboBox_SelectionChanged;
            LoadLogData();
        }

        // Does not work 
        // TODO: finish the log viewer
        public void LoadLogData()
        {
            try
            {
                string cs = $"Data Source={GlobalVars.ProgramDataWithAssemblyName}\\{SelectLogFileComboBox.SelectedItem};";
                string stm = "SELECT * FROM 'Logs'";
                DataTable dataTable = new DataTable();

                using var con = new SQLiteConnection(cs);
                con.Open();

                using var cmd = new SQLiteCommand(stm, con);
                var rdr = cmd.ExecuteReader();
                dataTable.Load(rdr);

                LoggerDataGridView.Items.Clear();
                foreach (var row in dataTable.Rows)
                {
                    LoggerDataGridView.Items.Add(row);
                }
                LoggerDataGridView.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.Data + Environment.NewLine + ex.Source + Environment.NewLine +
                    ex.StackTrace);
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        private void SelectLogFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadLogData();
        }

        private void DeleteLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectLogFileComboBox.SelectedItem != null && SelectLogFileComboBox.Items.Count > 1 && SelectLogFileComboBox.SelectedItem.ToString() != System.IO.Path.GetFileName(GlobalVars.LoggerPath))
                {
                    var file = GetAllLogFilePaths().Where(s => s.Contains(SelectLogFileComboBox.SelectedItem.ToString())).First();
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        SelectLogFileComboBox.ItemsSource = GetAllLogFileNames();
                        SelectLogFileComboBox.SelectedItem = System.IO.Path.GetFileName(GlobalVars.LoggerPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public enum LoggerType
        {
            Simple,
            Extended
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogData();
        }
    }
}
