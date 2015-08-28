using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Support.Security
{
	public interface IUpdateCrossDatabaseView
	{
		void Execute(string analyticsDbConnectionString, string aggDatabase);
	}

	public class UpdateCrossDatabaseView : IUpdateCrossDatabaseView
	{
		public void Execute(string analyticsDbConnectionString, string aggDatabase)
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