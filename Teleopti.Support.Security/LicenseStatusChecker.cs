using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

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
                    command.CommandText = "SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('-360')";
                    var versionCount = (int)command.ExecuteScalar();
                    if (versionCount > 0)
                    {
                        Console.WriteLine("The Check has been done.");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        return;
                    }
                }

                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    ILicenseService service = null;
                    
                    var numberOfActiveAgents = new PersonRepository(uow).NumberOfActiveAgents();
                    var statusXml = new LicenseStatusXml
                                        {
                                            StatusOk = true,
                                            NumberOfActiveAgents = numberOfActiveAgents,
                                            CheckDate = DateTime.Today.Date,
                                            LastValidDate = DateTime.Today.Date.AddDays(1),
                                            DaysLeft = 30
                                        };
                    try
                    {
                        service = new XmlLicenseService(new LicenseRepository(uow), numberOfActiveAgents);
                    }
                    catch (TooManyActiveAgentsException)
                    {
                        statusXml.StatusOk = false;
                        statusXml.LastValidDate = DateTime.Today.Date.AddDays(30);
                    }
                    if (service != null)
                    {
                        statusXml.AlmostTooMany = service.IsThisAlmostTooManyActiveAgents(numberOfActiveAgents);
                    }
                    
                    var status = new LicenseStatus { XmlString = statusXml.XmlDocument.OuterXml };
                    new LicenseStatusRepository(uow).Add(status);
                    uow.PersistAll();
                }
                command = connection.CreateCommand();
                command.CommandText = string.Format(CultureInfo.InvariantCulture,
                                                    "INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('-360','7.1.360.1',GetDate(),'{0}')",
                                                    Environment.UserName);
                
                command.ExecuteNonQuery();
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}