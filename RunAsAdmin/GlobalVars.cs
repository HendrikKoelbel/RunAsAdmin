using Config.Net;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace RunAsAdmin
{
    public class GlobalVars
    {

        #region AutoUpdater Informations
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

        #region Logger
        /// <summary>
        /// Returns the ProgramData\%AppName%\Logger.log file path
        /// </summary>
        public static string LoggerPath => Path.Combine(ProgramData, Assembly.GetEntryAssembly().GetName().Name) + "\\Logger.db";
        public ILogger Log => new LoggerConfiguration().WriteTo.LiteDB(LoggerPath).CreateLogger(); 
        #endregion

        #region Settings
        /// <summary>
        /// Returns the ProgramData\%AppName%\Settings.json file path
        /// </summary>
        public static string SettingsPath => Path.Combine(ProgramData, Assembly.GetEntryAssembly().GetName().Name) + "\\Settings.json";
        /// <summary>
        /// Creates the ConfigurationBuilder with the ISettings Interface to get or set settings
        /// </summary>
        public static ISettings SettingsHelper { get; set; } = new ConfigurationBuilder<ISettings>()
                    .UseJsonFile(SettingsPath)
                    .Build();
        /// <summary>
        /// Contains all setting values
        /// </summary>
        public interface ISettings
        {
            [Option(Alias = "Design.Theme", DefaultValue = null)]
            string Theme { get; set; }
            [Option(Alias = "Design.Accent", DefaultValue = null)]
            string Accent { get; set; }
            [Option(Alias = "UserData.Username", DefaultValue = null)]
            public string Username { get; set; }
            [Option(Alias = "UserData.Password", DefaultValue = null)]
            public string Password { get; set; }
            [Option(Alias = "UserData.Domain", DefaultValue = null)]
            public string Domain { get; set; }
        }
        #endregion

        #region SpecialFolder Paths
        /// <summary>
        ///  Returns the AppData\Roaming Path
        /// </summary>
        public static string AppDataRoaming => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        /// <summary>
        ///  Returns the AppData\Local Path
        /// </summary>
        public static string AppDataLocal => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        /// <summary>
        /// Returns the ProgramData Path
        /// </summary>
        public static string ProgramData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        /// <summary>
        /// Returns the local Temp path
        /// </summary>
        public static string TempPath => Path.GetTempPath();
        /// <summary>
        /// Returns a temoprary file path thats created with a unique name and 0 bytes
        /// </summary>
        public static string TempFile => Path.GetTempFileName();
        /// <summary>
        /// Returns the AppData\Local\%AppName% Path
        /// </summary>
        public static string AppDataWithAssemblyName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name);
        /// <summary>
        /// Returns the AppData\Local\Temp\%AppName% Path
        /// </summary>
        public static string TempPathWithAssemblyName => Path.Combine(Path.GetTempPath(), Assembly.GetEntryAssembly().GetName().Name);

        /// <summary>
        /// Method to get special folders
        /// </summary>
        /// <param name="specialFolder">Selection of the special folder</param>
        /// <returns>Returns the folder path of the selected special folder</returns>
        public static string GetPath(Environment.SpecialFolder specialFolder)
        {
            return Environment.GetFolderPath(specialFolder);
        }
        /// <summary>
        /// Returns only the path of the application
        /// </summary>
        public static string BasePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        /// <summary>
        /// Returns the file path of the application
        /// </summary>
        public static string ExecutablePath => Assembly.GetExecutingAssembly().Location;
        #endregion

        #region MahApps Style
        /// <summary>
        /// Theme Light/Dark enum
        /// </summary>
        public enum Themes
        {
            Light,
            Dark
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
