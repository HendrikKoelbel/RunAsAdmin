using LiteDB;
using LiteDB.Engine;
using System;
using System.Collections.Generic;
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
    public partial class LogViewerWindow : Window
    {
        public LogViewerWindow()
        {
            InitializeComponent();
        }

        private List<LoggerValues> GetAll()
        {
            var list = new List<LoggerValues>();
            using (var db = new LiteDatabase(GlobalVars.LoggerPath))
            {
                var col = db.GetCollection<LoggerValues>("log");
                foreach (LoggerValues _id in col.FindAll())
                {
                    list.Add(_id);
                }
            }
            return list;
        }

        public void DisplayPresetData()
        {
            LoggerDataGridView.ItemsSource = GetAll();
        }
    }

    public class LoggerValues
    {

    }
}
