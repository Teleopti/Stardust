using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Interfaces.Infrastructure;

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
			var connectionStringBuilder = new SqlConnectionStringBuilder(commandLineArgument.DestinationConnectionString);

			if (!string.IsNullOrEmpty(commandLineArgument.AggDatabase))
			{
				CrossDatabaseViewUpdate.Execute(commandLineArgument);
			}
			else
			{
				var scheduleConverter =
				new ConvertSchedule(new IPersonAssignmentConverter[]
				{
					new PersonAssignmentAuditDateSetter(connectionStringBuilder),
					new PersonAssignmentDateSetter(connectionStringBuilder)
				});

				scheduleConverter.ExecuteAllConverters();
				ForecasterDateAdjustment.Execute(commandLineArgument);
				PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
				PasswordEncryption.Execute(commandLineArgument);
				LicenseStatusChecker.Execute(commandLineArgument);
			}
			Environment.ExitCode = 0;
        }
    }
}
