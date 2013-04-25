using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;

namespace Teleopti.Support.Security
{
	class Program
	{
		private static readonly ICommandLineCommand PasswordEncryption = new PasswordEncryption();
        private static readonly ICommandLineCommand ForecasterDateAdjustment = new ForecasterDateAdjustment();
        private static readonly ICommandLineCommand PersonFirstDayOfWeekSetter = new PersonFirstDayOfWeekSetter();
        private static readonly ICommandLineCommand LicenseStatusChecker = new LicenseStatusChecker();
		private static readonly ICommandLineCommand CrossDatabaseViewUpdate = new CrossDatabaseViewUpdate();

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
				ForecasterDateAdjustment.Execute(commandLineArgument);
				PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
				PasswordEncryption.Execute(commandLineArgument);
				LicenseStatusChecker.Execute(commandLineArgument);
			}
			Environment.ExitCode = 0;
        }

		private static void setPersonAssignmentDate(CommandLineArgument commandLineArgument)
		{
			var allPersonAndTimeZone = new FetchPersonIdAndTimeZone(commandLineArgument.DestinationConnectionString).ForAllPersons();
			using (var conn = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				conn.Open();
				foreach (var personAndTimeZone in allPersonAndTimeZone)
				{
					using (var tx = conn.BeginTransaction())
					{
						var personId = personAndTimeZone.Item1;
						var timeZone = personAndTimeZone.Item2;
						new PersonAssignmentAuditDateSetter(tx).Execute(personId, timeZone);
						new PersonAssignmentDateSetter(tx).Execute(personId, timeZone);
						tx.Commit();
					}
				}
			}
		}
	}
}
