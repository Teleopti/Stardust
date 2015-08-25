using System.Data;
using System.Data.SqlClient;
using log4net;

namespace Teleopti.Support.Security
{
	internal class DelayedDataConvert : ICommandLineCommand
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public int Execute(IDatabaseArguments commandLineArgument)
		{
			//Select database version 
			using (SqlConnection connection = new SqlConnection(commandLineArgument.AnalyticsDbConnectionString))
			{
				connection.Open();

				//Check version
				SqlCommand command;
				using (command = connection.CreateCommand())
				{
					command.CommandType = CommandType.StoredProcedure;
                    //this will take some time. In case a rollback kicks in after 20 mins, this will effectively be a ~1 hour transaction
                    command.CommandTimeout = 1200; //20min
                    //check if old tables exists
				    int oldTablesCount = 0;
                    log.Debug("Check for delayed data convert ...");
                    command.CommandText = "mart.delayed_jobs_table_check";
                    oldTablesCount = AddRowsAffected(command, oldTablesCount);
                    if (oldTablesCount> 0) 
				    {
                        log.Debug("Add jobs to delayed table ...");
                        int rowsAffected = 0;
					    command.CommandText = "mart.fact_queue_etl_job_delayed";
					    rowsAffected = AddRowsAffected(command, rowsAffected);
                        command.CommandText = "mart.fact_imported_queue_etl_job_delayed";
                        rowsAffected = AddRowsAffected(command, rowsAffected);
					    command.CommandText = "mart.fact_agent_etl_job_delayed";
					    rowsAffected = AddRowsAffected(command, rowsAffected);
                        command.CommandText = "mart.fact_agent_queue_etl_job_delayed";
                        rowsAffected = AddRowsAffected(command, rowsAffected);
					    command.CommandText = "mart.main_schedule_etl_job_delayed";
					    rowsAffected = AddRowsAffected(command, rowsAffected);
					    log.Debug(rowsAffected + " jobs to delayed table. Done!");

					    log.Debug("Converting Data");
					    log.Debug("\tfact_queue ...");
					    command.CommandText = "mart.etl_execute_delayed_job";
					    command.Parameters.Add(new SqlParameter("@stored_procedure", "mart.etl_fact_queue_load"));
					    rowsAffected = AddRowsAffected(command, 0);
					    command.Parameters.Clear();
					    log.Debug("\tfact_queue converted " + rowsAffected + " rows.");

                        log.Debug("Converting Data");
                        log.Debug("\tfact_queue imported queues...");
                        command.CommandText = "mart.etl_execute_delayed_job";
                        command.Parameters.Add(new SqlParameter("@stored_procedure", "mart.etl_imported_queues_fact_queue_reload"));
                        rowsAffected = AddRowsAffected(command, 0);
                        command.Parameters.Clear();
                        log.Debug("\timported queues fact_queue converted " + rowsAffected + " rows.");

					    log.Debug("\tfact_agent ...");
					    command.CommandText = "mart.etl_execute_delayed_job";
					    command.Parameters.Add(new SqlParameter("@stored_procedure", "mart.etl_fact_agent_load"));
					    rowsAffected = AddRowsAffected(command, 0);
					    command.Parameters.Clear();
					    log.Debug("\tfact_agent converted " + rowsAffected + " rows.");

                        log.Debug("\tfact_agent_queue ...");
                        command.CommandText = "mart.etl_execute_delayed_job";
                        command.Parameters.Add(new SqlParameter("@stored_procedure", "mart.etl_fact_agent_queue_load"));
                        rowsAffected = AddRowsAffected(command, 0);
                        command.Parameters.Clear();
                        log.Debug("\tfact_agent_queue converted " + rowsAffected + " rows.");
                    
                        log.Debug("\tfact_schedule ...");
					    command.CommandText = "mart.etl_execute_delayed_job";
					    command.Parameters.Add(new SqlParameter("@stored_procedure", "mart.main_convert_fact_schedule_ccc8_run"));
					    rowsAffected = AddRowsAffected(command, 0);
					    command.Parameters.Clear();
					    log.Debug("\tfact_schedule converted " + rowsAffected + " rows.");

                        log.Debug("\tRemove old tables if possible ...");
                        command.CommandText = "mart.remove_old_tables";
				        rowsAffected = 0;
                        rowsAffected = AddRowsAffected(command, rowsAffected);
                        command.Parameters.Clear();
                        log.Debug("\tOld tables removed " + rowsAffected + " out of 6.");
                     }
				}
			}

			return 0;
		}

		public int AddRowsAffected(SqlCommand command, int previousCount)
		{
			int rowsAffected = command.ExecuteNonQuery();
			if (rowsAffected < 0)
			{
				rowsAffected = 0;
			}
			return rowsAffected + previousCount;
		}
	}
}