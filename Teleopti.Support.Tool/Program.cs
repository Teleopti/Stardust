using System;
using System.Globalization;
using System.IO;
using Teleopti.Support.Code.Tool;
using log4net;
using log4net.Config;

namespace Teleopti.Support.Tool
{
    static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RefreshConfigFile));
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            if (args.Length.Equals(0))
                new MainForm().ShowDialog();
            else
            {
                var commandLineArgument = new CommandLineArgument(args);
                if (commandLineArgument.ShowHelp)
                    new HelpWindow(commandLineArgument.Help).ShowDialog();
                else
                {
                    var file = "ConfigFiles.txt";
                    if (commandLineArgument.Mode.ToUpper(CultureInfo.InvariantCulture).Equals("DEPLOY"))
                        file = "DeployConfigFiles.txt";
                    if (commandLineArgument.Mode.ToUpper(CultureInfo.InvariantCulture).Equals("TEST"))
                        file = "BuildServerConfigFiles.txt";
                    var reader = new SettingsReader();
                    var settings = reader.GetSearchReplaceList(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"settings.txt")));
                    var refresher = new RefreshConfigFile(new ConfigFileTagReplacer(), new MachineKeyChecker());
                    try
                    {
                        refresher.ReadLinesFromString(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ConfigFiles\" + file)), settings, false);
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.Message);
                        Logger.Error("Failed to process the config files", e);
                    }
                    
                }
            }

        }
    }
}
