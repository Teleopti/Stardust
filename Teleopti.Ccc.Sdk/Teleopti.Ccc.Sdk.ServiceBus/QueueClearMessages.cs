using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public static class QueueClearMessages
	{
		public static void ClearMessages(string connectionString, string queueName)
		{
			var executor = new CloudSafeSqlExecute();
			executor.Run(() =>
			{
				var connection = new SqlConnection(connectionString);
				connection.Open();
				return connection;
			}, connection =>
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "Queue.CustomClearMessages";
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.AddWithValue("@QueueName", queueName);
					command.ExecuteNonQuery();
				}
			});
		}
	}
}
