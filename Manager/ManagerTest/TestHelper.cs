using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Stardust.Manager;
using Stardust.Manager.Models;

namespace ManagerTest
{
	public class TestHelper
	{
		private readonly ManagerConfiguration _managerConfiguration;

		public TestHelper(ManagerConfiguration managerConfiguration)
		{
			_managerConfiguration = managerConfiguration;
		}

		public void AddDeadNode(string nodeUrl)
		{
			const string commandString = @"INSERT INTO [Stardust].[WorkerNode] (Id, Url, Heartbeat, Alive) VALUES(@id,@url,@heartbeat,0)";
			using (var sqlConnection = new SqlConnection(_managerConfiguration.ConnectionString))
			{
				sqlConnection.Open();
				using (var workerNodeCommand = new SqlCommand(commandString, sqlConnection))
				{
					workerNodeCommand.Parameters.AddWithValue("@id", Guid.NewGuid());
					workerNodeCommand.Parameters.AddWithValue("@url", nodeUrl);
					workerNodeCommand.Parameters.AddWithValue("@heartbeat", DateTime.UtcNow.AddMinutes(-3));
					workerNodeCommand.ExecuteNonQuery();
				}
			}
		}

		public IList<WorkerNode> GetAllNodes()
		{
			var nodes = new List<WorkerNode>();
			const string commandString = @"SELECT * FROM [Stardust].[WorkerNode]";
			using (var sqlConnection = new SqlConnection(_managerConfiguration.ConnectionString))
			{
				sqlConnection.Open();
				using (var workerNodeCommand = new SqlCommand(commandString, sqlConnection))
				{
					using (var sqlDataReader = workerNodeCommand.ExecuteReader())
					{
						if (!sqlDataReader.HasRows) return nodes;
						while (sqlDataReader.Read())
						{
							var node = new WorkerNode
							{
								Url = new Uri(sqlDataReader.GetString(sqlDataReader.GetOrdinal("Url"))),
								Id = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("Id")),
								Heartbeat = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Heartbeat")),
								Alive = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("Alive"))
							};
							nodes.Add(node);
						}
					}
				}
			}
			return nodes;
		}
	}
}
