using System.Data;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;

namespace Teleopti.Support.Security.Library
{
	public class UpgradeRunner
	{
		private static readonly ICommandLineCommand passwordEncryption = new PasswordEncryption();
		private static readonly ICommandLineCommand forecasterDateAdjustment = new ForecasterDateAdjustment();
		private static readonly ICommandLineCommand personFirstDayOfWeekSetter = new PersonFirstDayOfWeekSetter();
		private static readonly ICommandLineCommand licenseStatusChecker = new LicenseStatusChecker();
		private static readonly ICommandLineCommand crossDatabaseViewUpdate = new CrossDatabaseViewUpdate(new UpdateCrossDatabaseView());
		private static readonly ICommandLineCommand delayedDataConvert = new DelayedDataConvert();
		private static readonly ICommandLineCommand reportTextCommand = new ReportTextsCommand();
		private static readonly ICommandLineCommand dayOffCodeFixer = new DayOffCodeFixer();
		private static readonly ICommandLineCommand dayOffIndexFixer = new DayOffIndexFixer();
		private static readonly ICommandLineCommand analyticsReportableScenarioFixer = new AnalyticsReportableScenarioFixer();

		private static readonly ILog log = LogManager.GetLogger(typeof(UpgradeRunner));
		private IUpgradeLog _upgradeLog;

		public UpgradeRunner(IUpgradeLog upgradeLog)
		{
			_upgradeLog = upgradeLog ?? new NullLog();
		}

		public void Upgrade(UpgradeCommand command)
		{
			if (command.CheckTenantConnectionStrings)
			{
				onlyCheckTenantConnections(command);
				return;
			}

			var currentTenantSession = TenantUnitOfWorkManager.Create(command.ApplicationConnectionString);

			Upgrade(command, currentTenantSession, currentTenantSession);
		}

		private static void onlyCheckTenantConnections(UpgradeCommand command)
		{
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(command.TenantStoreConnectionString);
			var checker = new CheckTenantConnectionStrings(tenantUnitOfWorkManager, tenantUnitOfWorkManager);
			checker.CheckConnectionStrings(command.TenantStoreConnectionString);
		}

		public void Upgrade(
			UpgradeCommand command,
			ITenantUnitOfWork tenantUnitOfWork,
			ICurrentTenantSession currentTenantSession)
		{
			var updateTenantData = new UpdateTenantData(tenantUnitOfWork, currentTenantSession);
			updateTenantData.UpdateTenantConnectionStrings(command.ApplicationConnectionStringToStore, command.AnalyticsConnectionStringToStore);
			updateTenantData.RegenerateTenantPasswords();
			if (!string.IsNullOrEmpty(command.AggDatabase))
			{
				//this if needs to be here to be able to run this from freemium (no anal db)
				reportTextCommand.Execute(command);
				crossDatabaseViewUpdate.Execute(command);
				delayedDataConvert.Execute(command);
			}

			if (!string.IsNullOrEmpty(command.AnalyticsConnectionString))
			{
				dayOffCodeFixer.Execute(command);
				dayOffIndexFixer.Execute(command);
			}

			setPersonAssignmentDate(command);
			removeDuplicateAssignments(command);
			forecasterDateAdjustment.Execute(command);
			personFirstDayOfWeekSetter.Execute(command);
			passwordEncryption.Execute(command);
			licenseStatusChecker.Execute(command);
			convertDayOffToNewStructure(command);
			analyticsReportableScenarioFixer.Execute(command);
		}

		private void convertDayOffToNewStructure(UpgradeCommand command)
		{
			const string proc = "[dbo].[DayOffConverter]";
			writeLog("Converting DayOffs ...");
			callProcInSeparateTransaction(command, proc);
			writeLog("Converting DayOffs. Done!");
		}

		private void removeDuplicateAssignments(UpgradeCommand command)
		{
			const string proc = "[dbo].[MergePersonAssignments]";
			writeLog("RemoveDuplicateAssignments ...");
			callProcInSeparateTransaction(command, proc);
			writeLog("RemoveDuplicateAssignments. Done!");
		}

		private void callProcInSeparateTransaction(UpgradeCommand command, string proc)
		{
			using (var conn = new SqlConnection(command.ApplicationConnectionString))
			{
				conn.Open();
				conn.InfoMessage += sqlConnectionInfoMessage;
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

		private void sqlConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			writeLog(e.Message);
		}

		private bool personAssignmentsConverted(UpgradeCommand command)
		{
			var numberOfNotConvertedCommand =
				@"select COUNT(*) as cnt from dbo.PersonAssignment pa
							where pa.[Date]=@baseDate";

			using (var conn = new SqlConnection(command.ApplicationConnectionString))
			{
				conn.Open();
				using (var cmd = new SqlCommand(numberOfNotConvertedCommand, conn))
				{
					cmd.Parameters.AddWithValue("@baseDate", PersonAssignmentDateSetter.DateOfUnconvertedSchedule2);
					if ((int) cmd.ExecuteScalar() > 0)
					{
						return false;
					}
				}
			}

			writeLog("No person assignment conversion needed");
			return true;
		}

		private void setPersonAssignmentDate(UpgradeCommand command)
		{
			if (personAssignmentsConverted(command))
				return;

			//expects all schedules having thedate set to 1800-1-1
			var allPersonAndTimeZone = new FetchPersonIdAndTimeZone(command.ApplicationConnectionString).ForAllPersons();
			int counter = allPersonAndTimeZone.Count();
			int i = 0;
			writeLog("Converting schedule data for " + counter + " agents");

			var personAssignmentSetter = new PersonAssignmentDateSetter();
			using (var conn = new SqlConnection(command.ApplicationConnectionString))
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

					i++;
					;
					if ((i % 1000) == 0)
					{
						writeLog("   agents left: " + (counter - i));
					}
				}
			}

			writeLog("Converting schedule data. Done!");
		}

		private void writeLog(string message)
		{
			log.Debug(message);
			_upgradeLog.Write(message); //, level.DisplayName);
		}

		public void SetLogger(IUpgradeLog upgradeLog)
		{
			_upgradeLog = upgradeLog;
		}
	}
}