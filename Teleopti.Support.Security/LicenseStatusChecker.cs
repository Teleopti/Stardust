using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.Infrastructure.Licensing;
using log4net.Config;
using log4net;

namespace Teleopti.Support.Security
{
    public class LicenseStatusChecker : ICommandLineCommand
    {
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        public int Execute(CommandLineArgument commandLineArgument)
        {
            //Select database version 
	        using (var connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
	        {

		        connection.Open();

		        //Check version
		        SqlCommand command;
		        using (command = connection.CreateCommand())
		        {
			        command.CommandText = "SELECT COUNT(*) FROM dbo.LicenseStatus";
			        var versionCount = (int) command.ExecuteScalar();
			        if (versionCount > 0)
			        {
				        return 0;
			        }
		        }

		        log.Debug("LicenseStatusChecker ...");
		        var status = new LicenseStatusXml
		        {
			        StatusOk = true,
			        NumberOfActiveAgents = 1,
			        CheckDate = DateTime.Today.Date,
			        LastValidDate = DateTime.Today.Date.AddDays(1),
			        DaysLeft = 30
		        };

		        var value = status.GetNewStatusDocument().OuterXml;

		        command = connection.CreateCommand();
		        command.CommandText = string.Format(CultureInfo.InvariantCulture,
			        "INSERT INTO dbo.LicenseStatus (Id, XmlString) VALUES (NewID(),'{0}')",
			        value);

		        command.ExecuteNonQuery();

	        }
	        log.Debug("LicenseStatusChecker. Done!");
	        return 0;
        }
    }
}