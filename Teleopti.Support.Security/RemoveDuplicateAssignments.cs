using System.Data.SqlClient;

namespace Teleopti.Support.Security
{
	public class RemoveDuplicateAssignments : ICommandLineCommand
	{
		public int Execute(CommandLineArgument commandLineArgument)
		{
			using (var connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				connection.Open();
				var cmd = new SqlCommand("exec [dbo].[MergePersonAssignments]", connection);
				return cmd.ExecuteNonQuery();
			}
		}
	}
}