using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Support.Library;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.DBManager
{
	class Program
	{
		static int Main(string[] args)
		{
			var loggerSuffix = "";
			if (args.Any(a => a.Contains("-O" + DatabaseType.TeleoptiCCC7.GetName())))
				loggerSuffix = "App";
			else if (args.Any(a => a.Contains("-O" + DatabaseType.TeleoptiAnalytics.GetName())))
				loggerSuffix = "Analytics";
			else if (args.Any(a => a.Contains("-O" + DatabaseType.TeleoptiCCCAgg.GetName())))
				loggerSuffix = "Agg";

			var log = new FileLogger(loggerSuffix);
			var env = new WfmInstallationEnvironment();
			try
			{
				log.Write($"Teleopti Database Manager version {Assembly.GetExecutingAssembly().GetName().Version}");
				log.Write($"Was called with args: {string.Join(" ", args)}");
				if (args.Length > 0 && args.Length < 20)
				{
					return new DatabasePatcher(log, env)
						.Run(PatchCommand.ParseCommandLine(args));
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
					Console.Out.WriteLine($"-O[{string.Join("|", Enum.GetNames(typeof(DatabaseType)))}]");
					Console.Out.WriteLine("-C Creates a database with name from -D switch.");
					Console.Out.WriteLine("-T Upgrade database.");
					Console.Out.WriteLine("-L[sqlUserName:sqlUserPassword] Will create a sql user that the application will use when running. Mandatory while using -C or -R");
					Console.Out.WriteLine("-W[Local windows Group] Will create a Windows Group that the application will use when running. . Mandatory while using -C or -R");
					Console.Out.WriteLine("-F Path to where dbmanager runs from");
					Console.Out.WriteLine("-R Apply permissions");
					return 0;
				}
			}
			catch (Exception e)
			{
				log.Write("An error occurred:");
				log.Write(e.ToString());
				return -1;
			}
			finally
			{
				log.Dispose();
			}
		}
	}
}