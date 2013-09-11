using System;
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
			int ExitCode = 0;

			var commandLineArgument = new CommandLineArgument(args);
			if (!string.IsNullOrEmpty(commandLineArgument.AggDatabase))
			{
				ExitCode=CrossDatabaseViewUpdate.Execute(commandLineArgument);
			}
			else
			{
				setPersonAssignmentDate(commandLineArgument);
				ExitCode += RemoveDuplicateAssignments.Execute(commandLineArgument);
				ExitCode += ForecasterDateAdjustment.Execute(commandLineArgument);
				ExitCode += PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
				ExitCode += PasswordEncryption.Execute(commandLineArgument);
				ExitCode += LicenseStatusChecker.Execute(commandLineArgument);
				convertDayOffToNewStructure(commandLineArgument);
				initAuditData(commandLineArgument);
			}
			Environment.ExitCode = ExitCode;
        }

		private static void initAuditData(CommandLineArgument commandLineArgument)
		{
			const string proc = "[Auditing].[TryInitAuditTables]";
			Console.WriteLine("Init Audit tables ...");
			callProcInSeperateTransaction(commandLineArgument, proc);
			Console.WriteLine("Done");
		}

		private static void convertDayOffToNewStructure(CommandLineArgument commandLineArgument)
		{
			const string proc = "[dbo].[DayOffConverter]";
			Console.WriteLine("Converting DayOff ...");
			callProcInSeperateTransaction(commandLineArgument, proc);
			Console.WriteLine("Done");
		}

		private static void callProcInSeperateTransaction(CommandLineArgument commandLineArgument, string proc)
		{
			using (var conn = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				conn.Open();
				using (var tx = conn.BeginTransaction())
				{
					using (var cmd = new SqlCommand(proc, conn, tx))
					{
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
