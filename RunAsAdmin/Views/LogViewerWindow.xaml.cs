using LiteDB;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
                var files = Directory.GetFiles(GlobalVars.PublicDocumentsWithAssemblyName, "Logger*.db").Select(System.IO.Path.GetFileName).ToList();
                foreach (var file in files)
                {
                    list.Add(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message} \n{ex.StackTrace}");
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
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message} \n{ex.StackTrace}");
            }
            return list;
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

        private List<LogModel> GetAll()
        {
            List<LogModel> list = new List<LogModel>().OrderBy(x => x._t.TimeOfDay).ToList();
            string conString = $"Filename={GlobalVars.ProgramDataWithAssemblyName}\\{SelectLogFileComboBox.SelectedItem};Connection=shared";
            using var db = new LiteDatabase(conString);
            var Items = db.GetCollection<LogModel>("log");
            foreach (LogModel Item in Items.FindAll())
            {
                list.Add(Item);
            }
            return list;
        }

        // Does not work 
        // TODO: finish the log viewer
        public void LoadLogData()
        {
            try
            {
                // Load the simple logger view
                LoggerDataGridView.ItemsSource = GetAll();
                // All column headers are overwritten with the DisplayName value of the property
                LogModel lm = new LogModel();
                var props = lm.GetType().GetProperties();
                for (int i = 0; i < props.Count(); i++)
                {
                    LoggerDataGridView.Columns[i].Header = props[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                }
                LoggerDataGridView.ItemsSource = GetAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message} \n{ex.StackTrace}");
            }
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
                        SelectLogFileComboBox.SelectedItem = Path.GetFileName(GlobalVars.LoggerPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message} \n{ex.StackTrace}");
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
            [DisplayName("Log ID")]
            public ObjectId _id { get; set; }
            [DisplayName("Date and Time")]
            public DateTime _t { get; set; }
            [DisplayName("Year")]
            public int _ty { get; set; }
            [DisplayName("Month")]
            public int _tm { get; set; }
            [DisplayName("Day")]
            public int _td { get; set; }
            [DisplayName("Week")]
            public int _tw { get; set; }
            [DisplayName("Message")]
            public string _m { get; set; }
            [DisplayName("Log Message")]
            public string _mt { get; set; }
            [DisplayName("ID")]
            public string _i { get; set; }
            [DisplayName("Log Level")]
            public string _l { get; set; }
            [DisplayName("Error Message")]
            public string _x { get; set; }
        }
    }
}
