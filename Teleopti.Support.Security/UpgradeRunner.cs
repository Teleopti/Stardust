using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Config;
using log4net.Core;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Support.Security
{
	public class UpgradeRunner
	{
		private static readonly ICommandLineCommand PasswordEncryption = new PasswordEncryption();
		private static readonly ICommandLineCommand ForecasterDateAdjustment = new ForecasterDateAdjustment();
		private static readonly ICommandLineCommand PersonFirstDayOfWeekSetter = new PersonFirstDayOfWeekSetter();
		private static readonly ICommandLineCommand LicenseStatusChecker = new LicenseStatusChecker();
		private static readonly ICommandLineCommand CrossDatabaseViewUpdate = new CrossDatabaseViewUpdate(new UpdateCrossDatabaseView());
		private static readonly ICommandLineCommand DelayedDataConvert = new DelayedDataConvert();
		private static readonly ICommandLineCommand reportTextCommand = new ReportTextsCommand();
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
		public IUpgradeLog Logger = new NullLog();

		public void Upgrade(IDatabaseArguments databaseArguments)
		{
			AppDomain.CurrentDomain.UnhandledException += appDomainUnhandledException;

			XmlConfigurator.Configure();
			//Console.WriteLine("Please be patient, don't close this window!");
			//Console.WriteLine("");
			//logToLog("Starting Teleopti.Support.Security");
			//logToLog("Was called with args: " + string.Join(" ", args));

			try
			{
				
				var tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(databaseArguments.ApplicationDbConnectionString);
				var updateTenantData = new UpdateTenantData(tenantUnitOfWorkManager);
				updateTenantData.UpdateTenantConnectionStrings(databaseArguments.ApplicationDbConnectionStringToStore, databaseArguments.AnalyticsDbConnectionStringToStore);
				updateTenantData.RegenerateTenantPasswords();
				if (!string.IsNullOrEmpty(databaseArguments.AggDatabase))
				{
					//this if needs to be here to be able to run this from freemium (no anal db)
					reportTextCommand.Execute(databaseArguments);
					CrossDatabaseViewUpdate.Execute(databaseArguments);
					DelayedDataConvert.Execute(databaseArguments);
				}
				setPersonAssignmentDate(databaseArguments);
				removeDuplicateAssignments(databaseArguments);
				ForecasterDateAdjustment.Execute(databaseArguments);
				PersonFirstDayOfWeekSetter.Execute(databaseArguments);
				PasswordEncryption.Execute(databaseArguments);
				LicenseStatusChecker.Execute(databaseArguments);
				convertDayOffToNewStructure(databaseArguments);
				initAuditData(databaseArguments);
			}
			catch (Exception e)
			{
				handleError(e);
			}

			Thread.Sleep(TimeSpan.FromSeconds(3));
			logToLog("Teleopti.Support.Security successful", Level.Debug);
			Environment.ExitCode = 0;
		}

		private void initAuditData(IDatabaseArguments commandLineArgument)
		{
			const string proc = "[Auditing].[TryInitAuditTables]";
			logToLog("Re-init Schedule history ...", Level.Debug);
			callProcInSeparateTransaction(commandLineArgument, proc);
			logToLog("Re-init Schedule history. Done!", Level.Debug);
		}

		private void convertDayOffToNewStructure(IDatabaseArguments commandLineArgument)
		{
			const string proc = "[dbo].[DayOffConverter]";
			logToLog("Converting DayOffs ...", Level.Debug);
			callProcInSeparateTransaction(commandLineArgument, proc);
			logToLog("Converting DayOffs. Done!", Level.Debug);
		}

		private void removeDuplicateAssignments(IDatabaseArguments commandLineArgument)
		{
			const string proc = "[dbo].[MergePersonAssignments]";
			logToLog("RemoveDuplicateAssignments ...", Level.Debug);
			callProcInSeparateTransaction(commandLineArgument, proc);
			logToLog("RemoveDuplicateAssignments. Done!", Level.Debug);
		}

		private void callProcInSeparateTransaction(IDatabaseArguments commandLineArgument, string proc)
		{
			using (var conn = new SqlConnection(commandLineArgument.ApplicationDbConnectionString))
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

		private void _sqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			logToLog(e.Message, Level.Debug);
		}

		private void setPersonAssignmentDate(IDatabaseArguments commandLineArgument)
		{
			//expects all schedules having thedate set to 1800-1-1
			var allPersonAndTimeZone = new FetchPersonIdAndTimeZone(commandLineArgument.ApplicationDbConnectionString).ForAllPersons();
			int counter = allPersonAndTimeZone.Count();
			int i = 0;
			logToLog("Converting schedule data for " + counter + " agents", Level.Debug);

			var personAssignmentSetter = new PersonAssignmentDateSetter();
			using (var conn = new SqlConnection(commandLineArgument.ApplicationDbConnectionString))
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
						logToLog("   agents left: " + (counter - i), Level.Debug);
					}
				}
			}
			logToLog("Converting schedule data. Done!", Level.Debug);
		}

		private void appDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			handleError((Exception)e.ExceptionObject);
		}

		private void handleError(Exception e)
		{
			logToLog("Teleopti.Support.Security has exited with fatal error: " + e.Message, Level.Fatal);
			log.Fatal(e.Message);
			log.Fatal(e.StackTrace);
			Thread.Sleep(TimeSpan.FromSeconds(5));
			Environment.Exit(1);
		}

		private void logToLog(string message, Level level)
		{
			if(level.Equals(Level.Debug))
				log.Debug(message);
			if (level.Equals(Level.Fatal))
				log.Fatal(message);

			Logger.Write(message,level.DisplayName);
      }
		internal class NullLog : IUpgradeLog
		{
			public void Write(string message)
			{

			}

			public void Write(string message, string level)
			{

			}

			public void Dispose()
			{

			}
		}
	}
}
