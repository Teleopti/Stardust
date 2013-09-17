using System;
using System.Globalization;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.Tool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
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
                    var reader = new SettingsReader();
                    var settings = reader.GetSearchReplaceList(System.IO.File.ReadAllText(@"settings.txt"));
                    var refresher = new RefreshConfigFile(new ConfigFileTagReplacer(), new MachineKeyChecker());
                    refresher.ReadLinesFromString(System.IO.File.ReadAllText(@"ConfigFiles\" + file), settings, false);
                }
            }

        }
    }
}
