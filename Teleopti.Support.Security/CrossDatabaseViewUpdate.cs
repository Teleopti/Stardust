using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net.Config;
using log4net;

namespace Teleopti.Support.Security
{
	internal class CrossDatabaseViewUpdate : ICommandLineCommand
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public int Execute(CommandLineArgument commandLineArgument)
		{
			log.Debug("Link Analytics to Agg datatbase ...");
			//Select database version 
			using (SqlConnection connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				connection.Open();

				//Check version
				SqlCommand command;
				using (command = connection.CreateCommand())
				{
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "mart.sys_crossdatabaseview_target_update";
					command.Parameters.Add(new SqlParameter("@defaultname", "TeleoptiCCCAgg"));
					command.Parameters.Add(new SqlParameter("@customname", commandLineArgument.AggDatabase));
					command.ExecuteNonQuery();

					command.CommandText = "mart.sys_crossdatabaseview_load";
					command.Parameters.Clear();
					command.ExecuteNonQuery();

				}
			}
			log.Debug("Link Analytics to Agg datatbase. Done!");
			return 0;
		}
	}
}