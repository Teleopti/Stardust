﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Teleopti.Support.Code.Tool;
using log4net;
using log4net.Config;

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
            // *********************************************************************************
            // Don't remove this, used for showing groups in a ListView /Henry
            System.Windows.Forms.Application.EnableVisualStyles();
            // *********************************************************************************

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
                    var refreshRunner = new RefreshConfigsRunner(new SettingsFileManager(new SettingsReader()),
                                                                 new RefreshConfigFile(new ConfigFileTagReplacer(),
                                                                                       new MachineKeyChecker()));
                    refreshRunner.RefreshThem(commandLineArgument.Mode);
                }
            }
        }
    }
}