using System;
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
                    var refresher = new RefreshConfigFile(new ConfigFileTagReplacer(commandLineArgument));
                    refresher.ReadLinesFromString(System.IO.File.ReadAllText(@"ConfigFiles\ConfigFiles.txt"));
                }
            }

        }
    }
}
