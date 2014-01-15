using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Security;
using log4net.Config;
using log4net;

namespace Teleopti.Support.Security
{
    internal class PasswordEncryption : ICommandLineCommand
    {
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
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
                    log.Debug("Could not open Sql Connection. Error message: " + ex.Message);
                    return 1;
                }
                catch (InvalidOperationException ex)
                {
                    log.Debug("Could not open Sql Connection. Error message: " + ex.Message);
                    return 1;
                }
                //Check version
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('-290')";
                var versionCount = (int)command.ExecuteScalar();
                if (versionCount > 0)
                {
                    return 0;
                }

                //Open transaction
                SqlTransaction transaction = null;
                try
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);


					using (SqlDataAdapter daPerson = new SqlDataAdapter("SELECT * FROM dbo.ApplicationAuthenticationInfo WHERE Password<>''", connection))
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

                                var specification = new IsPasswordEncryptedSpecification();
                                var encryption = new OneWayEncryption();
                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    object passwordObj = row["Password"];
                                    if (passwordObj != DBNull.Value)
                                    {
                                        string password = (string)passwordObj;
                                        if (!specification.IsSatisfiedBy(password))
                                        {
                                            //set new password
											row["Password"] = encryption.EncryptString(password);

											//update root object
											command = connection.CreateCommand();
											command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture,
																				"UPDATE dbo.Person SET Version=Version+1,UpdatedOn = GetUtcDate() WHERE Id='{0}'",
																				row["Person"]);
											command.Transaction = transaction;
											command.ExecuteNonQuery();
                                        }
                                    }
                                }
                                daPerson.Update(ds);
                            }
                        }
                        //Update databaseversion
                        command = connection.CreateCommand();
                        command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                                            "INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('-290','7.1.290.1',GetDate(),'{0}')",
                                                            Environment.UserName);
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();

                        //Commit!
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    log.Debug("Something went wrong! Error message: " + ex.Message);
	                return 1;
                }
            }

	        return 0;
        }
    }
}