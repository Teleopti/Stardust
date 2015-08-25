using System.Data;
using System.Data.SqlClient;
using log4net;

namespace Teleopti.Support.Security
{
	internal class CrossDatabaseViewUpdate : ICommandLineCommand
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public int Execute(IDatabaseArguments commandLineArgument)
		{
			log.Debug("Link Analytics to Agg datatbase ...");
			
			UpdateCrossDatabaseView.Execute(commandLineArgument.AnalyticsDbConnectionString, commandLineArgument.AggDatabase);
            log.Debug("Link Analytics to Agg datatbase. Done!");
			return 0;
		}
	}


	public static class UpdateCrossDatabaseView
	{
		public static void Execute(string analyticsDbConnectionString, string aggDatabase)
		{
			//Select database version 
			using (SqlConnection connection = new SqlConnection(analyticsDbConnectionString))
			{
				connection.Open();

				//Check version
				SqlCommand command;
				using (command = connection.CreateCommand())
				{
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "mart.sys_crossdatabaseview_target_update";
					command.Parameters.Add(new SqlParameter("@defaultname", "TeleoptiCCCAgg"));
					command.Parameters.Add(new SqlParameter("@customname", aggDatabase));
					command.ExecuteNonQuery();

					command.CommandText = "mart.sys_crossdatabaseview_load";
					command.Parameters.Clear();
					command.ExecuteNonQuery();

					command.CommandText = "mart.etl_job_intraday_settings_load";
					command.Parameters.Clear();
					command.ExecuteNonQuery();
				}
			}
		}
	}
}