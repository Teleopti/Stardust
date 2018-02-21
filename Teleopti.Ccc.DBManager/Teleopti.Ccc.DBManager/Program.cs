﻿using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.DBManager.Library;

namespace Teleopti.Ccc.DBManager
{
    class Program
    {
        static int Main(string[] args)
        {
	        var loggerSuffix = "";
	        if (args.Any(a => a.Contains("-O" + DatabaseType.TeleoptiCCC7.GetName())))
	        {
		        loggerSuffix = "App";
	        }
	        else if (args.Any(a => a.Contains("-O" + DatabaseType.TeleoptiAnalytics.GetName())))
	        {
		        loggerSuffix = "Analytics";
	        }
	        else if (args.Any(a => a.Contains("-O" + DatabaseType.TeleoptiCCCAgg.GetName())))
	        {
		        loggerSuffix = "Agg";
	        }
	        var logger = new FileLogger(loggerSuffix);
            try
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string versionNumber = version.ToString();

				logger.Write("Teleopti Database Manager version " + versionNumber);
				logger.Write("Was called with args: " + string.Join(" ", args));
				if (args.Length > 0 && args.Length < 20)
                {
                    var commandLineArgument = new CommandLineArgument(args);
					var patcher = new DatabasePatcher(logger);
	                return patcher.Run(commandLineArgument);
                }
                else
                {
                    Console.Out.WriteLine("Usage:");
                    Console.Out.WriteLine("DBManager.exe [switches]");
                    Console.Out.WriteLine("-S[Server name]");
                    Console.Out.WriteLine("-D[Database name]");
                    Console.Out.WriteLine("-U[User name]");
                    Console.Out.WriteLine("-P[Password]");
                    Console.Out.WriteLine("-E Uses Integrated Security, otherwise SQL Server security.");
                    string databaseTypeList = string.Join("|", Enum.GetNames(typeof(DatabaseType)));
                    Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "-O[{0}]", databaseTypeList));
                    Console.Out.WriteLine("-C Creates a database with name from -D switch.");
                    Console.Out.WriteLine("-L[sqlUserName:sqlUserPassword] Will create a sql user that the application will use when running. Mandatory while using -C or -R");
                    Console.Out.WriteLine("-W[Local windows Group] Will create a Windows Group that the application will use when running. . Mandatory while using -C or -R");
                    Console.Out.WriteLine("-F Path to where dbmanager runs from");
	                return 0;
                }
            }
            catch (Exception e)
            {
				logger.Write("An error occurred:");
				logger.Write(e.ToString());
                return -1;
            }
            finally
            {
				logger.Dispose();
            }
        }
	}
}