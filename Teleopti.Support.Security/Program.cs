using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Support.Security
{
	class Program
	{
		private static readonly ICommandLineCommand PasswordEncryption = new PasswordEncryption();
        private static readonly ICommandLineCommand ForecasterDateAdjustment = new ForecasterDateAdjustment();
        private static readonly ICommandLineCommand PersonFirstDayOfWeekSetter = new PersonFirstDayOfWeekSetter();
        private static readonly ICommandLineCommand LicenseStatusChecker = new LicenseStatusChecker();
		private static readonly ICommandLineCommand CrossDatabaseViewUpdate = new CrossDatabaseViewUpdate();
		private static readonly ICommandLineCommand RemoveDuplicateAssignments = new RemoveDuplicateAssignments();

		static void Main(string[] args)
		{
			var commandLineArgument = new CommandLineArgument(args);
			if (!string.IsNullOrEmpty(commandLineArgument.AggDatabase))
			{
				CrossDatabaseViewUpdate.Execute(commandLineArgument);
			}
			else
			{
				setPersonAssignmentDate(commandLineArgument);
				RemoveDuplicateAssignments.Execute(commandLineArgument);
				ForecasterDateAdjustment.Execute(commandLineArgument);
				PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
				PasswordEncryption.Execute(commandLineArgument);
				LicenseStatusChecker.Execute(commandLineArgument);
				convertDayOffToNewStructure(commandLineArgument);
				initAuditData(commandLineArgument);
			}
			Environment.ExitCode = 0;
        }

		private static void initAuditData(CommandLineArgument commandLineArgument)
		{
			const string proc = "[Auditing].[TryInitAuditTables]";
			callProcInSeparateTransaction(commandLineArgument, proc);
		}

		private static void convertDayOffToNewStructure(CommandLineArgument commandLineArgument)
		{
			const string proc = "[dbo].[DayOffConverter]";
			callProcInSeparateTransaction(commandLineArgument, proc);
		}

		private static void callProcInSeparateTransaction(CommandLineArgument commandLineArgument, string proc)
		{
			using (var conn = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				conn.Open();
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

		private static void setPersonAssignmentDate(CommandLineArgument commandLineArgument)
		{
			//expects all schedules having thedate set to 1800-1-1
			var allPersonAndTimeZone = new FetchPersonIdAndTimeZone(commandLineArgument.DestinationConnectionString).ForAllPersons();
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
				}
			}
		}
	}
}
