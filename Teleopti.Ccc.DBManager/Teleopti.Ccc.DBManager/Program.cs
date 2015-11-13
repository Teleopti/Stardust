using System;
using System.Globalization;
using System.Reflection;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager
{
    class Program
    {
    	private static IUpgradeLog _logger;
		
        static int Main(string[] args)
        {
            try
            {
				_logger = new FileLogger();

                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string versionNumber = version.ToString();
               
                logWrite("Teleopti Database Manager version " + versionNumber);
				logWrite("Was called with args: " + string.Join(" ", args));
				if (args.Length > 0 && args.Length < 20)
                {
                    var commandLineArgument = new CommandLineArgument(args);
					var patcher = new DatabasePatcher(_logger);
	                patcher.Run(commandLineArgument);
                }
                else
                {
                    Console.Out.WriteLine("Usage:");
                    Console.Out.WriteLine("DBManager.exe [switches]");
                    Console.Out.WriteLine("-S[Server name]");
                    Console.Out.WriteLine("-D[Database name]");
                    Console.Out.WriteLine("-U[User name]");
                    Console.Out.WriteLine("-P[Password]");
                    Console.Out.WriteLine("-N[Target build number]");
                    Console.Out.WriteLine("-E Uses Integrated Security, otherwise SQL Server security.");
                    string databaseTypeList = string.Join("|", Enum.GetNames(typeof(DatabaseType)));
                    Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "-O[{0}]", databaseTypeList));
                    Console.Out.WriteLine("-C Creates a database with name from -D switch.");
                    Console.Out.WriteLine("-L[sqlUserName:sqlUserPassword] Will create a sql user that the application will use when running. Mandatory while using -C or -R");
                    Console.Out.WriteLine("-W[Local windows Group] Will create a Windows Group that the application will use when running. . Mandatory while using -C or -R");
                    Console.Out.WriteLine("-B[Business Unit]");
                    Console.Out.WriteLine("-F Path to where dbmanager runs from");
                }
                return 0;
            }
            catch (Exception e)
            {
                logWrite("An error occurred:");
                logWrite(e.ToString());
                return -1;
            }
            finally
            {
                _logger.Dispose();
            }
        }

		private static void logWrite(string s)
		{
			_logger.Write(s);
		}
	}
}