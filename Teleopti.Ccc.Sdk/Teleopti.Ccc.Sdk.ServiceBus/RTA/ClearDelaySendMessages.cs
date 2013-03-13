using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public static class ClearDelaySendMessages
	{
		public static void ClearMessages(string connectionString)
		{
			const string query = "DELETE FROM Queue.Messages WHERE Headers LIKE '%rta%time-to-send%'";
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = query;
				command.ExecuteNonQuery();
			}
		}
	}
}
