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
using ControlzEx.Theming;
using MahApps.Metro.Controls;

namespace RunAsAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            SwitchThemeToggle.Toggled -= SwitchThemeToggle_Toggled;
            SwitchThemeToggle.IsOn = true ? GlobalVars.SettingsHelper.Theme == ThemeManager.BaseColorDark : false;
            SwitchThemeToggle.Toggled += SwitchThemeToggle_Toggled;
            SwitchAccentComboBox.SelectionChanged -= SwitchAccentComboBox_SelectionChanged;
            SwitchAccentComboBox.ItemsSource = Enum.GetValues(typeof(GlobalVars.Accents));
            SwitchAccentComboBox.SelectedIndex = SwitchAccentComboBox.Items.IndexOf((GlobalVars.Accents)Enum.Parse(typeof(GlobalVars.Accents), GlobalVars.SettingsHelper.Accent));
            SwitchAccentComboBox.SelectionChanged += SwitchAccentComboBox_SelectionChanged;
        }

        private void SwitchThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SwitchThemeToggle.IsOn == true)
                {
                    // Switch Theme
                    ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
                    // Display current theme on the SwitchLabel
                    SwitchThemeToggle.OnContent = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Save current theme in the settings
                    GlobalVars.SettingsHelper.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Log this event
                    GlobalVars.Loggi.Information($"Theme was changed from {ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)).BaseColorScheme} to {ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme}");
                }
                else
                {
                    // Switch Theme
                    ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
                    // Display current theme on the SwitchLabel
                    SwitchThemeToggle.OffContent = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Save current theme in the settings
                    GlobalVars.SettingsHelper.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                    // Log this event
                    GlobalVars.Loggi.Information($"Theme was changed from {ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)).BaseColorScheme} to {ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme}");
                }
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
        private void SwitchThemeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ThemeManager.Current.ChangeTheme(Application.Current, ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)));
                GlobalVars.SettingsHelper.Theme = ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme;
                GlobalVars.Loggi.Information($"Theme was changed from {ThemeManager.Current.GetInverseTheme(ThemeManager.Current.DetectTheme(Application.Current)).BaseColorScheme} to {ThemeManager.Current.DetectTheme(Application.Current).BaseColorScheme}");
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }

        private void SwitchAccentComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, SwitchAccentComboBox.SelectedItem.ToString());
                GlobalVars.SettingsHelper.Accent = SwitchAccentComboBox.SelectedItem.ToString();
                GlobalVars.Loggi.Information($"Accent was changed to {SwitchAccentComboBox.SelectedItem.ToString()}");
            }
            catch (Exception ex)
            {
                GlobalVars.Loggi.Error(ex, ex.Message);
            }
        }
    }
}
