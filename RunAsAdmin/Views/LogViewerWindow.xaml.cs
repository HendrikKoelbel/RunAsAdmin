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
using System.Reflection;
using System.Runtime.Remoting.Messaging;
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
        private void LogViewerWindow_ContentRendered(object sender, EventArgs e)
        {
            DisplayPresetData();
        }

        private List<ExtendedLoggerModel> GetExtendedLoggerData()
        {
            var list = new List<ExtendedLoggerModel>();
            try
            {
                using var db = new LiteDatabase(@"Filename=" + GlobalVars.LoggerPathWithDate + ";connection=shared");
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
            var list = new List<SimpleLoggerModel>();
            try
            {
                using var db = new LiteDatabase(@"Filename=" + GlobalVars.LoggerPathWithDate + ";connection=shared");
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

        public void DisplayPresetData()
        {
            LoggerDataGridView.ItemsSource = GetSimpleLoggerData();
            SimpleOrExtendedComboBox.ItemsSource = Enum.GetValues(typeof(LoggerType));
            SimpleOrExtendedComboBox.SelectedItem = LoggerType.Simple;
        }

        private void SimpleOrExtendedComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoggerDataGridView.ItemsSource = null;
            LoggerDataGridView.Items.Clear();
            switch (SimpleOrExtendedComboBox.SelectedItem)
            {
                case LoggerType.Simple:
                    LoggerDataGridView.ItemsSource = GetSimpleLoggerData();
                    break;
                case LoggerType.Extended:
                    LoggerDataGridView.ItemsSource = GetExtendedLoggerData();
                    break;
            }
            LoggerDataGridView.Items.Refresh();
        }
    }

    public enum LoggerType
    {
        Simple,
        Extended
    }

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
