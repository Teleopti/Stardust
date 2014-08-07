using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public static class QueueClearMessages
	{
		public static void ClearMessages(string connectionString, string queueName, string subQueue = null)
		{
			using (var connection = new SqlConnection(connectionString))
			using (var command = connection.CreateCommand())
			{
				connection.Open();
				command.CommandText = "Queue.CustomClearMessages";
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@QueueName", queueName);
				command.Parameters.AddWithValue("@Subqueue", subQueue);
				command.ExecuteNonQuery();
			}
		}
	}
}
