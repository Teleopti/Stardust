﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;

namespace Teleopti.Support.Security
{
    public class PersonFirstDayOfWeekSetter : ICommandLineCommand
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
                    command.CommandText = "SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('-340')";
                    var versionCount = (int) command.ExecuteScalar();
                    if (versionCount > 0)
                    {
                        Console.WriteLine("The Persons are up to date.");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        return;
                    }
                }

                //Load all timezones for Skills and Workloads
                var timeZoneDictionary = new Dictionary<Guid, int>();

                using (command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT Id, ISNULL(Culture,0) FROM Person";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int firstDayOfWeek = getFirstDayFromCulture(reader.GetInt32(1));
                            timeZoneDictionary.Add(reader.GetGuid(0),firstDayOfWeek);
                        }
                    }
                    Console.WriteLine("Created a dictionary for persons with first day of work week.");
                }

                //Open transaction
                SqlTransaction transaction = null;
                try
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                    Console.WriteLine("Updating persons...");
                    using (var daPerson = new SqlDataAdapter("SELECT * FROM dbo.Person", connection))
                    {
                        daPerson.SelectCommand.Transaction = transaction;
                        using (var builder = new SqlCommandBuilder(daPerson))
                        {
                            daPerson.UpdateCommand = builder.GetUpdateCommand();
                            daPerson.DeleteCommand = builder.GetDeleteCommand();
                            daPerson.InsertCommand = builder.GetInsertCommand();

                            //Update FirstDayOfWeek
                            using (var ds = new DataSet())
                            {
                                ds.Locale = CultureInfo.InvariantCulture;
                                daPerson.Fill(ds);

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    
                                    row["FirstDayOfWeek"] = timeZoneDictionary[(Guid) row["Id"]];
                                    row["Version"] = ((int) row["Version"]) + 1;
                                    row["UpdatedOn"] = DateTime.UtcNow;
                                }
                                daPerson.Update(ds);
                            }
                        }
                    }
                    Console.WriteLine("Done updating persons.");
                    Console.WriteLine("Updating database version...");
                    command = connection.CreateCommand();
                    command.CommandText = string.Format(CultureInfo.InvariantCulture,
                                                        "INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('-340','7.1.340.1',GetDate(),'{0}')",
                                                        Environment.UserName);
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                    Console.WriteLine("Done updating database version.");

                    //Commit!
                    Console.WriteLine("Committing transaction...");
                    transaction.Commit();
                    Console.WriteLine("Transaction successfully committed! Done.");
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    Console.WriteLine("Something went wrong! Error message: {0}", ex.Message);
                }
                finally
                {
                    // done with using
                    //connection.Dispose();
                }
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        private static int getFirstDayFromCulture(int culture)
        {
            if (culture == 0)
                return 1;

            return (int)CultureInfo.GetCultureInfo(culture).DateTimeFormat.FirstDayOfWeek;

        }
    }
}