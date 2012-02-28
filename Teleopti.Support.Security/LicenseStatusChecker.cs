using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Support.Security
{
    public class LicenseStatusChecker : ICommandLineCommand
    {
        public void Execute(CommandLineArgument commandLineArgument)
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
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine("Could not open Sql Connection. Error message: {0}", ex.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return;
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
                        return;
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

                var value = status.XmlDocument.OuterXml;
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
                    return;
                }
                
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}