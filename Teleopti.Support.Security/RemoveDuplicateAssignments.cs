using System.Data.SqlClient;
using System;
using log4net.Config;
using log4net;

namespace Teleopti.Support.Security
{
	public class RemoveDuplicateAssignments : ICommandLineCommand
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public int Execute(CommandLineArgument commandLineArgument)
		{
			using (var connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				var returnvalue = 0;
				try
				{
					log.Debug("RemoveDuplicateAssignments ...");
					connection.Open();
					var cmd = new SqlCommand("exec [dbo].[MergePersonAssignments]", connection);
					returnvalue = cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
						log.Debug("Something went wrong! Error message: " + ex.Message);
				}
				finally
				{
					log.Debug("RemoveDuplicateAssignments. Done!");
				}
				return returnvalue;
			}
		}
	}
}