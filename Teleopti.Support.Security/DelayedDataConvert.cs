using System.Data;
using System.Data.SqlClient;
using log4net;

namespace Teleopti.Support.Security
{
	internal class DelayedDataConvert : ICommandLineCommand
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
					log.Debug("Add jobs to delayed table ...");
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "mart.fact_queue_etl_job_delayed";
					command.ExecuteNonQuery();

					command.CommandText = "mart.main_schedule_etl_job_delayed";
					command.ExecuteNonQuery();
					log.Debug("Add jobs to delayed table. Done!");

					//this will taek some time. Fail after 20 mins
					command.CommandTimeout = 1200; //20min

					log.Debug("Converting 6 months fact_queue ...");
					command.CommandText = "mart.etl_execute_delayed_job";
					command.Parameters.Add(new SqlParameter("@stored_procedure", "mart.etl_fact_queue_load"));
					command.ExecuteNonQuery();
					command.Parameters.Clear();
					log.Debug("Converting 6 months fact_queue. Done!");

					log.Debug("Converting 1 months fact_schedule ...");
					command.CommandText = "mart.etl_execute_delayed_job";
					command.Parameters.Add(new SqlParameter("@stored_procedure", "mart.main_convert_fact_schedule_ccc8_run"));
					command.ExecuteNonQuery();
					command.Parameters.Clear();
					log.Debug("Converting 1 months fact_schedule. Done!");
				}
			}

			return 0;
		}
	}
}