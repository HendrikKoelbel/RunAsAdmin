using Castle.Components.DictionaryAdapter.Xml;
using LiteDB;
using LiteDB.Engine;
using MahApps.Metro.Controls;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Windows;
using System.Windows.Data;

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
        private void LogViewerWindow_ContentRendered(object sender, EventArgs e)
        {
            // Set the Enum values in the ComboBox and select the simple logger view
            SimpleOrExtendedComboBox.ItemsSource = Enum.GetValues(typeof(LoggerType));
            SelectLogFileComboBox.ItemsSource = GetAllLogFiles();
            SimpleOrExtendedComboBox.SelectedItem = LoggerType.Simple;
        }

        private static List<string> GetAllLogFiles()
        {
            var list = new List<string>();
            try
            {
                var files = Directory.GetFiles(GlobalVars.ProgramDataWithAssemblyName, "Logger*.db").Select(Path.GetFileName).ToList();
                foreach (var file in files)
                {
                    list.Add(file);
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
            return list;
        }
        private List<ExtendedLoggerModel> GetExtendedLoggerData()
        {
            var list = new List<ExtendedLoggerModel>().OrderBy(x => x._t.TimeOfDay).ToList();
            try
            {
                using var db = new LiteDatabase(@"Filename=" + GlobalVars.LoggerPath + ";connection=shared");
                var Items = db.GetCollection<ExtendedLoggerModel>("log");
                foreach (ExtendedLoggerModel Item in Items.FindAll())
                {
                    list.Add(Item);
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
            return list;
        }
        private List<SimpleLoggerModel> GetSimpleLoggerData()
        {
            var list = new List<SimpleLoggerModel>().OrderBy(o => o._t.TimeOfDay).ToList();
            try
            {
                using var db = new LiteDatabase(@"Filename=" + GlobalVars.LoggerPath + ";connection=shared");
                var Items = db.GetCollection<SimpleLoggerModel>("log");
                foreach (SimpleLoggerModel Item in Items.FindAll())
                {
                    list.Add(Item);
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
            return list;
        }

        private void SimpleOrExtendedComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Clear the DataGrid
            LoggerDataGridView.ItemsSource = null;
            LoggerDataGridView.Items.Clear();
            switch (SimpleOrExtendedComboBox.SelectedItem)
            {
                case LoggerType.Simple:
                    // Load the simple logger view
                    LoggerDataGridView.ItemsSource = GetSimpleLoggerData();
                    // All column headers are overwritten with the DisplayName value of the property
                    SimpleLoggerModel slm = new SimpleLoggerModel();
                    var prop1 = slm.GetType().GetProperties();
                    for (int i = 0; i < prop1.Count(); i++)
                    {
                        LoggerDataGridView.Columns[i].Header = prop1[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                    }
                    break;
                case LoggerType.Extended:
                    // Load the extended logger view
                    LoggerDataGridView.ItemsSource = GetExtendedLoggerData();
                    // All column headers are overwritten with the DisplayName value of the property
                    ExtendedLoggerModel elm = new ExtendedLoggerModel();
                    var prop2 = elm.GetType().GetProperties();
                    for (int i = 0; i < prop2.Count(); i++)
                    {
                        LoggerDataGridView.Columns[i].Header = prop2[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                    }
                    break;
            }
            // Refresh the DataGrid Items
            LoggerDataGridView.Items.Refresh();
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
        public DateTime _t { get; set; }
        [DisplayName("Message")]
        public string _m { get; set; }
        [DisplayName("Log Level")]
        public string _l { get; set; }
        [DisplayName("Error Message")]
        public string _x { get; set; }
    }
    // Extended model with all logging values
    public class ExtendedLoggerModel
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
