using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net.Config;
using log4net;

namespace Teleopti.Support.Security
{
    internal class ForecasterDateAdjustment : ICommandLineCommand
    {
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public int Execute(CommandLineArgument commandLineArgument)
        {
            //Select database version 
	        using (SqlConnection connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
	        {

		        connection.Open();

		        //Check version
		        SqlCommand command;
		        using (command = connection.CreateCommand())
		        {
			        command.CommandText = "SELECT COUNT(*) FROM dbo.DatabaseVersion WHERE BuildNumber=('-330')";
			        var versionCount = (int) command.ExecuteScalar();
			        if (versionCount > 0)
			        {
				        return 1;
			        }
		        }

		        log.Debug("ForecastDateAdjuster ...");

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
			        log.Debug("Created a dictionary for all skills and workloads with time zone information.");
		        }

		        //Open transaction
		        SqlTransaction transaction = null;

		        transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

		        log.Debug("Updating skill days...");
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
		        log.Debug("Done updating skill days.");

		        log.Debug("Updating validated days...");
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
						        DateTime skillDayDate = (DateTime) row["VolumeDayDate"];
						        row["VolumeDayDate"] = TimeZoneInfo.ConvertTimeFromUtc(skillDayDate,
							        timeZoneDictionary[
								        (Guid) row["Workload"]]);

						        row["Version"] = ((int) row["Version"]) + 1;
						        row["UpdatedOn"] = DateTime.UtcNow;
					        }
					        daPerson.Update(ds);
				        }
			        }
		        }
		        log.Debug("Done updating validated days.");

		        log.Debug("Updating workload days...");
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
						        DateTime skillDayDate = (DateTime) row["WorkloadDate"];
						        row["WorkloadDate"] = TimeZoneInfo.ConvertTimeFromUtc(skillDayDate,
							        timeZoneDictionary[
								        (Guid) row["Workload"]]);
					        }
					        daPerson.Update(ds);
				        }
			        }
		        }
		        log.Debug("Done updating workload days.");

		        log.Debug("Updating multisite days...");
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
						        DateTime skillDayDate = (DateTime) row["MultisiteDayDate"];
						        row["MultisiteDayDate"] = TimeZoneInfo.ConvertTimeFromUtc(skillDayDate,
							        timeZoneDictionary[
								        (Guid) row["Skill"]]);

						        row["Version"] = ((int) row["Version"]) + 1;
						        row["UpdatedOn"] = DateTime.UtcNow;
					        }
					        daPerson.Update(ds);
				        }
			        }
		        }
		        log.Debug("Done updating multisite days.");

		        log.Debug("Updating database version...");
		        command = connection.CreateCommand();
		        command.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture,
			        "INSERT INTO dbo.DatabaseVersion (BuildNumber,SystemVersion,AddedDate,AddedBy) VALUES ('-330','7.1.330.1',GetDate(),'{0}')",
			        Environment.UserName);
		        command.Transaction = transaction;
		        command.ExecuteNonQuery();
		        log.Debug("Done updating database version.");

		        //Commit!
		        log.Debug("Committing transaction...");
		        transaction.Commit();
		        log.Debug("Transaction successfully committed! Done.");

	        }
	        log.Debug("ForecastDateAdjuster. Done!");
	        return 0;
        }
    }
}