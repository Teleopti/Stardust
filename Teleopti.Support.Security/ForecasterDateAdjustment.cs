using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Teleopti.Support.Security
{
    internal class ForecasterDateAdjustment : ICommandLineCommand
    {
        public int Execute(CommandLineArgument commandLineArgument)
        {
            //Select database version 
            using (SqlConnection connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
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
                    command.CommandText = "SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('-330')";
                    var versionCount = (int) command.ExecuteScalar();
                    if (versionCount > 0)
                    {
                        Console.WriteLine("The database is up to date.");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        return 1;
                    }
                }

                //Load all timezones for Skills and Workloads
                var timeZoneDictionary = new Dictionary<Guid, TimeZoneInfo>();

                using (command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT Id,TimeZone FROM dbo.Skill UNION SELECT w.Id,s.TimeZone FROM dbo.Workload w INNER JOIN dbo.Skill s ON s.Id=w.Skill";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            timeZoneDictionary.Add(reader.GetGuid(0),
                                                   TimeZoneInfo.FindSystemTimeZoneById(reader.GetString(1)));
                        }
                    }
                    Console.WriteLine("Created a dictionary for all skills and workloads with time zone information.");
                }

                //Open transaction
                SqlTransaction transaction = null;
                try
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                    Console.WriteLine("Updating skill days...");
                    using (SqlDataAdapter daPerson = new SqlDataAdapter("SELECT * FROM dbo.SkillDay", connection))
                    {
                        daPerson.SelectCommand.Transaction = transaction;
                        using (SqlCommandBuilder builder = new SqlCommandBuilder(daPerson))
                        {
                            daPerson.UpdateCommand = builder.GetUpdateCommand();
                            daPerson.DeleteCommand = builder.GetDeleteCommand();
                            daPerson.InsertCommand = builder.GetInsertCommand();

                            //Update passwords
                            using (DataSet ds = new DataSet())
                            {
                                ds.Locale = System.Globalization.CultureInfo.InvariantCulture;
                                daPerson.Fill(ds);

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    DateTime skillDayDate = (DateTime) row["SkillDayDate"];
                                    row["SkillDayDate"] = TimeZoneInfo.ConvertTimeFromUtc(skillDayDate,
                                                                                          timeZoneDictionary[
                                                                                              (Guid) row["Skill"]]);

                                    row["Version"] = ((int) row["Version"]) + 1;
                                    row["UpdatedOn"] = DateTime.UtcNow;
                                }
                                daPerson.Update(ds);
                            }
                        }
                    }
                    Console.WriteLine("Done updating skill days.");

                    Console.WriteLine("Updating validated days...");
                    using (SqlDataAdapter daPerson = new SqlDataAdapter("SELECT * FROM dbo.ValidatedVolumeDay", connection))
                    {
                        daPerson.SelectCommand.Transaction = transaction;
                        using (SqlCommandBuilder builder = new SqlCommandBuilder(daPerson))
                        {
                            daPerson.UpdateCommand = builder.GetUpdateCommand();
                            daPerson.DeleteCommand = builder.GetDeleteCommand();
                            daPerson.InsertCommand = builder.GetInsertCommand();

                            //Update passwords
                            using (DataSet ds = new DataSet())
                            {
                                ds.Locale = System.Globalization.CultureInfo.InvariantCulture;
                                daPerson.Fill(ds);

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    DateTime skillDayDate = (DateTime)row["VolumeDayDate"];
                                    row["VolumeDayDate"] = TimeZoneInfo.ConvertTimeFromUtc(skillDayDate,
                                                                                           timeZoneDictionary[
                                                                                               (Guid)row["Workload"]]);

                                    row["Version"] = ((int)row["Version"]) + 1;
                                    row["UpdatedOn"] = DateTime.UtcNow;
                                }
                                daPerson.Update(ds);
                            }
                        }
                    }
                    Console.WriteLine("Done updating validated days.");

                    Console.WriteLine("Updating workload days...");
                    using (SqlDataAdapter daPerson = new SqlDataAdapter("SELECT * FROM dbo.WorkloadDayBase", connection))
                    {
                        daPerson.SelectCommand.Transaction = transaction;
                        using (SqlCommandBuilder builder = new SqlCommandBuilder(daPerson))
                        {
                            daPerson.UpdateCommand = builder.GetUpdateCommand();
                            daPerson.DeleteCommand = builder.GetDeleteCommand();
                            daPerson.InsertCommand = builder.GetInsertCommand();

                            //Update passwords
                            using (DataSet ds = new DataSet())
                            {
                                ds.Locale = System.Globalization.CultureInfo.InvariantCulture;
                                daPerson.Fill(ds);

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    DateTime skillDayDate = (DateTime)row["WorkloadDate"];
                                    row["WorkloadDate"] = TimeZoneInfo.ConvertTimeFromUtc(skillDayDate,
                                                                                          timeZoneDictionary[
                                                                                              (Guid)row["Workload"]]);
                                }
                                daPerson.Update(ds);
                            }
                        }
                    }
                    Console.WriteLine("Done updating workload days.");

                    Console.WriteLine("Updating multisite days...");
                    using (SqlDataAdapter daPerson = new SqlDataAdapter("SELECT * FROM dbo.MultisiteDay", connection))
                    {
                        daPerson.SelectCommand.Transaction = transaction;
                        using (SqlCommandBuilder builder = new SqlCommandBuilder(daPerson))
                        {
                            daPerson.UpdateCommand = builder.GetUpdateCommand();
                            daPerson.DeleteCommand = builder.GetDeleteCommand();
                            daPerson.InsertCommand = builder.GetInsertCommand();

                            //Update passwords
                            using (DataSet ds = new DataSet())
                            {
                                ds.Locale = System.Globalization.CultureInfo.InvariantCulture;
                                daPerson.Fill(ds);

                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    DateTime skillDayDate = (DateTime)row["MultisiteDayDate"];
                                    row["MultisiteDayDate"] = TimeZoneInfo.ConvertTimeFromUtc(skillDayDate,
                                                                                              timeZoneDictionary[
                                                                                                  (Guid)row["Skill"]]);

                                    row["Version"] = ((int)row["Version"]) + 1;
                                    row["UpdatedOn"] = DateTime.UtcNow;
                                }
                                daPerson.Update(ds);
                            }
                        }
                    }
                    Console.WriteLine("Done updating multisite days.");

                    Console.WriteLine("Updating database version...");
                    command = connection.CreateCommand();
                    command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                        "INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('-330','7.1.330.1',GetDate(),'{0}')",
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

	                return 1;
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

	        return 0;
        }
    }
}