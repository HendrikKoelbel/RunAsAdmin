using Config.Net;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace RunAsAdmin
{
    public class GlobalVars
    {
        #region Onova AutoUpdater Informations
        /// <summary>
        /// GitHub Username to the userprofile
        /// </summary>
        public static string GitHubUsername => "HendrikKoelbel";
        /// <summary>
        /// GitHub project name to the repository
        /// </summary>
        public static string GitHubProjectName => "RunAsAdmin";
        /// <summary>
        /// Asset name that will be downloaded from the Updater
        /// </summary>
        public static string GitHubAssetName => "RunAsAdmin.zip";
        #endregion

        #region Settings
        public static string SettingsPath => Path.GetTempPath() + Assembly.GetEntryAssembly().GetName().Name + "\\Settings.json";

        public static ISettings Settings { get; set; } = new ConfigurationBuilder<ISettings>()
                    .UseJsonFile(SettingsPath)
                    .Build();

        public interface ISettings
        {
            [Option(Alias = "Paths")]
            string LastUsedPath { get; set; }

            [Option(Alias = "Design.Theme", DefaultValue = null)]
            string Theme { get; set; }
            [Option(Alias = "Design.Accent", DefaultValue = null)]
            string Accent { get; set; }
        }
        #endregion

        #region MahApps Style
        /// <summary>
        /// Theme Dark/Light enum
        /// </summary>
        public enum Themes
        {
            Dark,
            Light,
        }
        /// <summary>
        /// All accent colors in one enum
        /// </summary>
        public enum Accents
        {
            Red,
            Green,
            Blue,
            Purple,
            Orange,
            Lime,
            Emerald,
            Teal,
            Cyan,
            Cobalt,
            Indigo,
            Violet,
            Pink,
            Magenta,
            Crimson,
            Amber,
            Yellow,
            Brown,
            Olive,
            Steel,
            Mauve,
            Taupe,
            Sienna
        }
        #endregion
    }
}
