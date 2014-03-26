using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using log4net.Config;
using log4net;

namespace Teleopti.Support.Security
{
    public class PersonFirstDayOfWeekSetter : ICommandLineCommand
    {
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.log.Debug(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.log.Debug(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
			        command.CommandText = "SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('-340')";
			        var versionCount = (int) command.ExecuteScalar();
			        if (versionCount > 0)
			        {
				        return 1;
			        }
		        }

		        log.Debug("PersonFirstDayOfWeekSetter ...");

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
					        timeZoneDictionary.Add(reader.GetGuid(0), firstDayOfWeek);
				        }
			        }
			        log.Debug("Created a dictionary for persons with first day of work week.");
		        }

		        //Open transaction
		        SqlTransaction transaction = null;

		        transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

		        log.Debug("Updating persons...");
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
		        log.Debug("Done updating persons.");
		        log.Debug("Updating database version...");
		        command = connection.CreateCommand();
		        command.CommandText = string.Format(CultureInfo.InvariantCulture,
			        "INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('-340','7.1.340.1',GetDate(),'{0}')",
			        Environment.UserName);
		        command.Transaction = transaction;
		        command.ExecuteNonQuery();
		        log.Debug("Done updating database version.");

		        //Commit!
		        log.Debug("Committing transaction...");
		        transaction.Commit();
		        log.Debug("Transaction successfully committed! Done.");

	        }
	        log.Debug("PersonFirstDayOfWeekSetter. Done!");
	        return 0;
        }

        private static int getFirstDayFromCulture(int culture)
        {
            if (culture == 0)
                return 1;

            return (int)CultureInfo.GetCultureInfo(culture).DateTimeFormat.FirstDayOfWeek;

        }
    }
}