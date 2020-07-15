﻿using LiteDB;
using MahApps.Metro.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

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

        #region ContentRendered
        private void LogViewerWindow_ContentRendered(object sender, EventArgs e)
        {
            // Set the Enum values in the ComboBox and select the simple logger view
            SimpleOrExtendedComboBox.ItemsSource = Enum.GetValues(typeof(LoggerType));
            SimpleOrExtendedComboBox.SelectedItem = CurrentLoggerTypeEnum;
            SelectLogFileComboBox.ItemsSource = GetAllLogFileNames();
            SelectLogFileComboBox.SelectedItem = Path.GetFileName(GlobalVars.LoggerPath);
        }
        #endregion

        #region Initialize DataGrid ItemSource
        private void ItemSourceClear()
        {
            LoggerDataGridView.ItemsSource = null;
            LoggerDataGridView.Items.Clear();
        }

        private void ItemSourceRefresh(IEnumerable itemSource = null)
        {
            if (itemSource != null)
                LoggerDataGridView.ItemsSource = itemSource;

            LoggerDataGridView.Items.Refresh();
        }

        private void ItemSourceAdd(IEnumerable itemSource = null)
        {
            LoggerDataGridView.ItemsSource = itemSource;
        }
        #endregion

        #region Logfile names and paths
        private static List<string> GetAllLogFileNames()
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
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        private static List<string> GetAllLogFilePaths()
        {
            var list = new List<string>();
            try
            {
                var files = Directory.GetFiles(GlobalVars.ProgramDataWithAssemblyName, "Logger*.db").ToList();
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

        #region Logger Data
        private List<ExtendedLoggerModel> GetExtendedLoggerData()
        {
            var list = new List<ExtendedLoggerModel>().OrderBy(x => x._t.TimeOfDay).ToList();
            try
            {
                CurrentLogPath = GetAllLogFilePaths().Where(s => s.Contains(SelectLogFileComboBox.SelectedItem.ToString())).First();
                using var db = new LiteDatabase(@"Filename=" + CurrentLogPath + ";connection=shared");
                var Items = db.GetCollection<ExtendedLoggerModel>("log");
                foreach (ExtendedLoggerModel Item in Items.FindAll())
                {
                    list.Add(Item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        private List<SimpleLoggerModel> GetSimpleLoggerData(string loggerPath = null)
        {
            var list = new List<SimpleLoggerModel>().OrderBy(o => o._t.TimeOfDay).ToList();
            try
            {
                CurrentLogPath = GetAllLogFilePaths().Where(s => s.Contains(SelectLogFileComboBox.SelectedItem.ToString())).First();
                using var db = new LiteDatabase(@"Filename=" + CurrentLogPath + ";connection=shared");
                var Items = db.GetCollection<SimpleLoggerModel>("log");
                foreach (SimpleLoggerModel Item in Items.FindAll())
                {
                    list.Add(Item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        #endregion

        #region Windowevents
        private void SimpleOrExtendedComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                switch (SimpleOrExtendedComboBox.SelectedItem)
                {
                    case LoggerType.Simple:
                        // Load the simple logger view
                        //LoggerDataGridView.ItemsSource = GetSimpleLoggerData((string)SelectLogFileComboBox.SelectedItem ?? null);
                        // All column headers are overwritten with the DisplayName value of the property
                        SimpleLoggerModel slm = new SimpleLoggerModel();
                        var prop1 = slm.GetType().GetProperties();
                        for (int i = 0; i < prop1.Count(); i++)
                        {
                            LoggerDataGridView.Columns[i].Header = prop1[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                        }
                        ItemSourceRefresh(GetSimpleLoggerData((string)SelectLogFileComboBox.SelectedItem ?? null));
                        break;
                    case LoggerType.Extended:
                        // Load the extended logger view
                        //LoggerDataGridView.ItemsSource = GetExtendedLoggerData((string)SelectLogFileComboBox.SelectedItem ?? null);
                        // All column headers are overwritten with the DisplayName value of the property
                        ExtendedLoggerModel elm = new ExtendedLoggerModel();
                        var prop2 = elm.GetType().GetProperties();
                        for (int i = 0; i < prop2.Count(); i++)
                        {
                            LoggerDataGridView.Columns[i].Header = prop2[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                        }
                        ItemSourceRefresh();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SelectLogFileComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                switch (SimpleOrExtendedComboBox.SelectedItem)
                {
                    case LoggerType.Simple:
                        // Load the simple logger view
                        //LoggerDataGridView.ItemsSource = GetSimpleLoggerData((string)SelectLogFileComboBox.SelectedItem ?? null);
                        // All column headers are overwritten with the DisplayName value of the property
                        SimpleLoggerModel slm = new SimpleLoggerModel();
                        var prop1 = slm.GetType().GetProperties();
                        for (int i = 0; i < prop1.Count(); i++)
                        {
                            LoggerDataGridView.Columns[i].Header = prop1[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                        }
                        ItemSourceRefresh(GetSimpleLoggerData((string)SelectLogFileComboBox.SelectedItem ?? null));
                        break;
                    case LoggerType.Extended:
                        // Load the extended logger view
                        //LoggerDataGridView.ItemsSource = GetExtendedLoggerData((string)SelectLogFileComboBox.SelectedItem ?? null);
                        // All column headers are overwritten with the DisplayName value of the property
                        ExtendedLoggerModel elm = new ExtendedLoggerModel();
                        var prop2 = elm.GetType().GetProperties();
                        for (int i = 0; i < prop2.Count(); i++)
                        {
                            LoggerDataGridView.Columns[i].Header = prop2[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                        }
                        ItemSourceRefresh();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DeleteLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectLogFileComboBox.SelectedItem != null)
                {
                    var file = GetAllLogFilePaths().Where(s => s.Contains(SelectLogFileComboBox.SelectedItem.ToString())).First();
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion
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
