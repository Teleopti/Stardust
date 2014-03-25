using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using log4net.Config;
using log4net;
using System.Linq;
using System.Threading;

namespace Teleopti.Support.Security
{
	class Program
	{
		private static readonly ICommandLineCommand PasswordEncryption = new PasswordEncryption();
        private static readonly ICommandLineCommand ForecasterDateAdjustment = new ForecasterDateAdjustment();
        private static readonly ICommandLineCommand PersonFirstDayOfWeekSetter = new PersonFirstDayOfWeekSetter();
        private static readonly ICommandLineCommand LicenseStatusChecker = new LicenseStatusChecker();
		private static readonly ICommandLineCommand CrossDatabaseViewUpdate = new CrossDatabaseViewUpdate();
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += appDomainUnhandledException;

			XmlConfigurator.Configure();
			Console.WriteLine("Please be patient, don't close this window!");
			Console.WriteLine("");
			log.Debug("Starting Teleopti.Support.Security");

			try
			{
				var commandLineArgument = new CommandLineArgument(args);
				if (!string.IsNullOrEmpty(commandLineArgument.AggDatabase))
				{
					CrossDatabaseViewUpdate.Execute(commandLineArgument);
				}
				else
				{
					setPersonAssignmentDate(commandLineArgument);
					removeDuplicateAssignments(commandLineArgument);
					ForecasterDateAdjustment.Execute(commandLineArgument);
					PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
					PasswordEncryption.Execute(commandLineArgument);
					LicenseStatusChecker.Execute(commandLineArgument);
					convertDayOffToNewStructure(commandLineArgument);
					initAuditData(commandLineArgument);
				}
			}
			catch (Exception e)
			{
				handleError(e);
			}
			
			Thread.Sleep(TimeSpan.FromSeconds(3));
			log.Debug("Teleopti.Support.Security successful");
			Environment.ExitCode = 0;
        }

		private static void initAuditData(CommandLineArgument commandLineArgument)
		{
			const string proc = "[Auditing].[TryInitAuditTables]";
			log.Debug("Re-init Schedule history ...");
			callProcInSeparateTransaction(commandLineArgument, proc);
			log.Debug("Re-init Schedule history. Done!");
		}

		private static void convertDayOffToNewStructure(CommandLineArgument commandLineArgument)
		{
			const string proc = "[dbo].[DayOffConverter]";
			log.Debug("Converting DayOffs ...");
			callProcInSeparateTransaction(commandLineArgument, proc);
			log.Debug("Converting DayOffs. Done!");
		}

		private static void removeDuplicateAssignments(CommandLineArgument commandLineArgument)
		{
			const string proc = "[dbo].[MergePersonAssignments]";
			log.Debug("RemoveDuplicateAssignments ...");
			callProcInSeparateTransaction(commandLineArgument, proc);
			log.Debug("RemoveDuplicateAssignments. Done!");
		}

		private static void callProcInSeparateTransaction(CommandLineArgument commandLineArgument, string proc)
		{
			using (var conn = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				conn.Open();
				conn.InfoMessage += _sqlConnection_InfoMessage;
				using (var tx = conn.BeginTransaction())
				{
					using (var cmd = new SqlCommand(proc, conn, tx))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.CommandTimeout = 1800; //30 min timeout + an additional 30 min rollback = 1 h
						cmd.ExecuteNonQuery();
					}
					tx.Commit();
				}
			}
		}

		private static void _sqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			log.Debug(e.Message);
		}

		private static void setPersonAssignmentDate(CommandLineArgument commandLineArgument)
		{
			//expects all schedules having thedate set to 1800-1-1
			var allPersonAndTimeZone = new FetchPersonIdAndTimeZone(commandLineArgument.DestinationConnectionString).ForAllPersons();
			int counter = allPersonAndTimeZone.Count();
			int i = 0;
			log.Debug("Converting schedule data for " + counter + " agents");

			var personAssignmentSetter = new PersonAssignmentDateSetter();
			using (var conn = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				conn.Open();
				foreach (var personAndTimeZone in allPersonAndTimeZone)
				{
					var personId = personAndTimeZone.Item1;
					var timeZone = personAndTimeZone.Item2;
					using (var tx = conn.BeginTransaction())
					{
						personAssignmentSetter.Execute(tx, personId, timeZone);
						tx.Commit();
					}
					i++; ;
					if ((i % 1000) == 0)
					{
						log.Debug("   agents left: " + (counter-i));
					}
				}
			}
			log.Debug("Converting schedule data. Done!");
		}

		private static void appDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			handleError((Exception)e.ExceptionObject);
		}

		private static void handleError(Exception e)
		{
			log.Fatal("Teleopti.Support.Security has exited with fatal error:");
			log.Fatal(e.Message);
			log.Fatal(e.StackTrace);
			Thread.Sleep(TimeSpan.FromSeconds(5));
			Environment.Exit(1);
		}
	}
}
