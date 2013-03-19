using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public static class QueueClearMessages
	{
		public static void ClearMessages(string connectionString, string queueName)
		{
			using (var connection = new SqlConnection(connectionString))
			using (var command = connection.CreateCommand())
			{
				connection.Open();			
				command.CommandText = "Queue.CustomClearMessages";
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.AddWithValue("@QueueName", queueName);
				command.ExecuteNonQuery();
			}
		}
	}
}
