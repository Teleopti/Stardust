using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Stardust.Manager.Extensions;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class WorkerNodeRepository : IWorkerNodeRepository
	{
		private readonly string _connectionString;
		private readonly RetryPolicy _retryPolicy;

		public WorkerNodeRepository(ManagerConfiguration managerConfiguration,
		                            RetryPolicyProvider retryPolicyProvider)
		{
			_connectionString = managerConfiguration.ConnectionString;
			_retryPolicy = retryPolicyProvider.GetPolicy();
		}

		public List<WorkerNode> GetAllWorkerNodes()
		{
			var listToReturn = new List<WorkerNode>();

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					var command = new SqlCommand
					{
						Connection = connection,
						CommandText = "SELECT Id, Url, Heartbeat, Alive " +
						              "FROM [Stardust].[WorkerNode]"
					};

					connection.OpenWithRetry(_retryPolicy);

					var reader = command.ExecuteReaderWithRetry(_retryPolicy);

					if (reader.HasRows)
					{
						var ordinalPositionForIdField = reader.GetOrdinal("Id");
						var ordinalPositionForUrlField = reader.GetOrdinal("Url");
						var ordinalPositionForAliveField = reader.GetOrdinal("Alive");
						var ordinalPositionForHeartbeatField = reader.GetOrdinal("Heartbeat");

						while (reader.Read())
						{
							var jobDefinition = new WorkerNode
							{
								Id = (Guid) reader.GetValue(ordinalPositionForIdField),
								Url = new Uri((string) reader.GetValue(ordinalPositionForUrlField)),
								Alive = (bool) reader.GetValue(ordinalPositionForAliveField),
								Heartbeat = (DateTime) reader.GetValue(ordinalPositionForHeartbeatField)
							};

							listToReturn.Add(jobDefinition);
						}
					}

					reader.Close();

					return listToReturn;
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);

				throw;
			}
		}

		public void AddWorkerNode(WorkerNode workerNode)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);

					var workerNodeCommand = connection.CreateCommand();

					workerNodeCommand.CommandText = "INSERT INTO [Stardust].[WorkerNode] " +
					                                "(Id, Url, Heartbeat, Alive) " +
					                                "VALUES(@Id, @Url, @Heartbeat, @Alive)";

					workerNodeCommand.Parameters.AddWithValue("@Id", workerNode.Id);
					workerNodeCommand.Parameters.AddWithValue("@Url", workerNode.Url.ToString());
					workerNodeCommand.Parameters.AddWithValue("@Heartbeat", workerNode.Heartbeat);
					workerNodeCommand.Parameters.AddWithValue("@Alive", workerNode.Alive);

					workerNodeCommand.ExecuteNonQueryWithRetry(_retryPolicy);
				}
			}
			catch (Exception exp)
			{
				if (exp.Message.Contains("UQ_WorkerNodes_Url"))
					return;

				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}

		public List<string> CheckNodesAreAlive(TimeSpan timeSpan)
		{
			var selectCommand = @"SELECT Id, 
										 Url, 
										 Heartbeat, 
										 Alive 
								 FROM [Stardust].[WorkerNode]";

			var updateCommandText = @"UPDATE [Stardust].[WorkerNode]
											SET Alive = @Alive
										WHERE Url = @Url";

			var deadNodes = new List<string>();
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);

					var ordinalPosForHeartBeat = 0;
					var ordinalPosForUrl = 0;

					var listOfObjectArray = new List<object[]>();

					using (var commandSelectAll = new SqlCommand(selectCommand, connection))
					{
						using (var readAllWorkerNodes = commandSelectAll.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (readAllWorkerNodes.HasRows)
							{
								ordinalPosForHeartBeat = readAllWorkerNodes.GetOrdinal("Heartbeat");
								ordinalPosForUrl = readAllWorkerNodes.GetOrdinal("Url");

								while (readAllWorkerNodes.Read())
								{
									var temp = new object[readAllWorkerNodes.FieldCount];
									readAllWorkerNodes.GetValues(temp);
									listOfObjectArray.Add(temp);
								}
							}
							readAllWorkerNodes.Close();
						}
					}

					if (listOfObjectArray.Any())
					{
						using (var trans = connection.BeginTransaction())
						{
							using (var commandUpdate = new SqlCommand(updateCommandText, connection, trans))
							{
								commandUpdate.Parameters.Add("@Alive", SqlDbType.Bit);
								commandUpdate.Parameters.Add("@Url", SqlDbType.NVarChar);

								foreach (var objectse in listOfObjectArray)
								{
									var heartBeatDateTime =
										(DateTime) objectse[ordinalPosForHeartBeat];
									var url = objectse[ordinalPosForUrl];
									var currentDateTime = DateTime.UtcNow;
									var dateDiff =
										(currentDateTime - heartBeatDateTime).TotalSeconds;
									if (dateDiff > timeSpan.TotalSeconds)
									{
										commandUpdate.Parameters["@Alive"].Value = false;
										commandUpdate.Parameters["@Url"].Value = url;
										commandUpdate.ExecuteNonQueryWithRetry(_retryPolicy);
										deadNodes.Add(url.ToString());
									}
								}
							}
							trans.Commit();
						}
					}
					connection.Close();
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return deadNodes;
		}

	public void RegisterHeartbeat(string nodeUri, bool updateStatus)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);

					var updateCommandText = @"UPDATE [Stardust].[WorkerNode] SET Heartbeat = @Heartbeat,
											Alive = @Alive
											WHERE Url = @Url";

					if (!updateStatus)
					{
						updateCommandText = @"UPDATE [Stardust].[WorkerNode] SET Heartbeat = @Heartbeat
											WHERE Url = @Url";
					}

					using (var command = new SqlCommand(updateCommandText, connection))
					{
						command.Parameters.Add("@Heartbeat", SqlDbType.DateTime).Value = DateTime.UtcNow;
						command.Parameters.Add("@Alive", SqlDbType.Bit).Value = true;
						command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = nodeUri;

						command.ExecuteNonQueryWithRetry(_retryPolicy);
					}
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}
	}
}