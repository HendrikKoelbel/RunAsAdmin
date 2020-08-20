using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RunAsAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für LogViewerWindow.xaml
    /// </summary>
    public partial class LogViewerWindow : MetroWindow
    {
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
            // Set the Enum values in the ComboBox and select the simple logger view
            SimpleOrExtendedComboBox.ItemsSource = Enum.GetValues(typeof(LoggerType));
            SimpleOrExtendedComboBox.SelectedItem = CurrentLoggerTypeEnum;
            SelectLogFileComboBox.ItemsSource = GetAllLogFileNames();
            SelectLogFileComboBox.SelectedItem = System.IO.Path.GetFileName(GlobalVars.LoggerPath);
        }

        private void SimpleOrExtendedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SelectLogFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
        // Simple model with some logging values
        public class SimpleLoggerModel
        {
            [DisplayName("Date and Time")]
            public DateTime Timestamp { get; set; }
            [DisplayName("Log Level")]
            public string Level { get; set; }
            [DisplayName("Exception")]
            public string Exception { get; set; }
            [DisplayName("Log Message")]
            public string RenderedMessage { get; set; }
        }
        // Extended model with all logging values
        public class ExtendedLoggerModel
        {
            [DisplayName("Log ID")]
            public int id { get; set; }
            [DisplayName("Date and Time")]
            public DateTime Timestamp { get; set; }
            [DisplayName("Log Level")]
            public string Level { get; set; }
            [DisplayName("Exception")]
            public string Exception { get; set; }
            [DisplayName("Log Message")]
            public string RenderedMessage { get; set; }
            [DisplayName("Properties")]
            public string Properties { get; set; }
        }
    }
}
