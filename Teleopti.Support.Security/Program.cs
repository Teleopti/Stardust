using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck;
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
		private static readonly IPersonAssignmentConverter PersonAssignmentAuditDateSetter = new PersonAssignmentAuditDateSetter();
		private static readonly IPersonAssignmentConverter PersonAssignmentDateSetter = new PersonAssignmentDateSetter();


        static void Main(string[] args)
        {
            var commandLineArgument = new CommandLineArgument(args);

			var connectionStringBuilder = new SqlConnectionStringBuilder(commandLineArgument.DestinationConnectionString);

			var result = PersonAssignmentDateSetter.Execute(connectionStringBuilder);
			if(result!= 0)
				Environment.Exit(result);
			result = PersonAssignmentAuditDateSetter.Execute(connectionStringBuilder);
			if (result != 0)
				Environment.Exit(result);

			if (!string.IsNullOrEmpty(commandLineArgument.AggDatabase))
				CrossDatabaseViewUpdate.Execute(commandLineArgument);

            if (!commandLineArgument.ForecasterMode &&
                !commandLineArgument.PersonUpdateMode &&
                !commandLineArgument.PasswordEncryptionMode &&
                !commandLineArgument.LicenseStatusMode &&
				string.IsNullOrEmpty(commandLineArgument.AggDatabase))
            {
                ForecasterDateAdjustment.Execute(commandLineArgument);
                PasswordEncryption.Execute(commandLineArgument);
                PersonFirstDayOfWeekSetter.Execute(commandLineArgument);
                LicenseStatusChecker.Execute(commandLineArgument);
                return;
            }

            if (commandLineArgument.ForecasterMode)
                ForecasterDateAdjustment.Execute(commandLineArgument);

            if (commandLineArgument.PersonUpdateMode)
                PersonFirstDayOfWeekSetter.Execute(commandLineArgument);

            if (commandLineArgument.PasswordEncryptionMode)
                PasswordEncryption.Execute(commandLineArgument);

            if (commandLineArgument.LicenseStatusMode)
                LicenseStatusChecker.Execute(commandLineArgument);

	        Environment.ExitCode = 0;
        }


    }
}
