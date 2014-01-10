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
				SqlCommand command;
				using (command = connection.CreateCommand())
				{
					try
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

					catch (Exception ex)
					{
						log.Debug("Something went wrong! Error message: " + ex.Message);
						return 1;
					}
					finally
					{
						// done with using
						//connection.Dispose();
					}
				}
			}
			log.Debug("Link Analytics to Agg datatbase. Done!");
			return 0;
		}
	}
}