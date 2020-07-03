using LiteDB;
using MahApps.Metro.Controls;
using Serilog;
using System;
using System.Collections.Generic;
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
            DisplayPresetData();
        }


        private List<LoggerModel> GetLoggerData()
        {
            var list = new List<LoggerModel>();
            // Closes the Logger connection
            try
            {
                using (var db = new LiteDatabase(GlobalVars.LoggerPathWithDate))
                {
                    var Items = db.GetCollection<LoggerModel>("log");
                    foreach (LoggerModel Item in Items.FindAll())
                    {
                        list.Add(Item);
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            return list;
        }

        public void DisplayPresetData()
        {
            LoggerDataGridView.ItemsSource = GetLoggerData();
        }
    }

    public class LoggerModel
    {
        public LiteDB.ObjectId _id { get; set; }
        public DateTime _t { get; set; }
        public int _ty { get; set; }
        public int _tm { get; set; }
        public int _td { get; set; }
        public int _tw { get; set; }
        public string _m { get; set; }
        public string _mt { get; set; }
        public string _i { get; set; }
        public string _l { get; set; }
        public string _x { get; set; }
    }
}
