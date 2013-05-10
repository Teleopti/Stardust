using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Support.Security
{
    public class LicenseStatusChecker : ICommandLineCommand
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        public int Execute(CommandLineArgument commandLineArgument)
        {
            //Select database version 
            using (var connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Could not open Sql Connection. Error message: {0}", ex.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return 1;
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("Could not open Sql Connection. Error message: {0}", ex.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return 1;
                }
                //Check version
                SqlCommand command;
                using (command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM dbo.LicenseStatus";
                    var versionCount = (int)command.ExecuteScalar();
                    if (versionCount > 0)
                    {
                        Console.WriteLine("The Check has been done.");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        return 0;
                    }
                }

                var status = new LicenseStatusXml
                                 {
                                     StatusOk = true,
                                     NumberOfActiveAgents = 1,
                                     CheckDate = DateTime.Today.Date,
                                     LastValidDate = DateTime.Today.Date.AddDays(1),
                                     DaysLeft = 30
                                 };

                var value = status.GetNewStatusDocument().OuterXml;
                try
                {
                    command = connection.CreateCommand();
                    command.CommandText = string.Format(CultureInfo.InvariantCulture,
                                                        "INSERT INTO dbo.LicenseStatus (Id, XmlString) VALUES (NewID(),'{0}')",
                                                        value);

                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Could not save the status: {0}", ex.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return 1;
                }
                
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

	        return 0;
        }
    }
}