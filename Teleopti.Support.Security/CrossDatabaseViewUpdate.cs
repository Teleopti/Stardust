using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Teleopti.Support.Security
{
	internal class CrossDatabaseViewUpdate : ICommandLineCommand
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
						Console.WriteLine("Something went wrong! Error message: {0}", ex.Message);
						return 1;
					}
					finally
					{
						// done with using
						//connection.Dispose();
					}
					Thread.Sleep(TimeSpan.FromSeconds(2));
				}
			}

			return 0;
		}
	}
}