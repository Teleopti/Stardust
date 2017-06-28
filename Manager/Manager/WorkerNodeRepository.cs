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


		public void AddWorkerNode(WorkerNode workerNode)
		{
			const string selectWorkerNodeCommand = "INSERT INTO [Stardust].[WorkerNode] (Id, Url, Heartbeat, Alive) VALUES(@Id, @Url, @Heartbeat, @Alive)";
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);

					using (var workerNodeCommand = new SqlCommand(selectWorkerNodeCommand, connection))
					{
						workerNodeCommand.Parameters.AddWithValue("@Id", workerNode.Id);
						workerNodeCommand.Parameters.AddWithValue("@Url", workerNode.Url.ToString());
						workerNodeCommand.Parameters.AddWithValue("@Heartbeat", workerNode.Heartbeat);
						workerNodeCommand.Parameters.AddWithValue("@Alive", workerNode.Alive);

						workerNodeCommand.ExecuteNonQueryWithRetry(_retryPolicy);
					}
				}
			}
			catch (Exception exp)
			{
				if (exp.Message.Contains("UQ_WorkerNodes_Url"))
				{
					using (var connection = new SqlConnection(_connectionString))
					{
						connection.OpenWithRetry(_retryPolicy);
						const string updateCommandText = @"UPDATE [Stardust].[WorkerNode] SET Heartbeat = @Heartbeat,
											Alive = @Alive
											WHERE Url = @Url";

						using (var command = new SqlCommand(updateCommandText, connection))
						{
							command.Parameters.Add("@Heartbeat", SqlDbType.DateTime).Value = DateTime.UtcNow;
							command.Parameters.Add("@Alive", SqlDbType.Bit).Value = true;
							command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = workerNode.Url.ToString();

							command.ExecuteNonQueryWithRetry(_retryPolicy);
						}
					}
					return;
				}

				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		public List<string> CheckNodesAreAlive(TimeSpan timeSpan)
		{
			const string selectCommand = @"SELECT Id, 
										 Url, 
										 Heartbeat, 
										 Alive 
								 FROM [Stardust].[WorkerNode] WHERE Alive = 1";

			const string updateCommandText = @"UPDATE [Stardust].[WorkerNode]
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

					var allAliveNodes = new List<object[]>();

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
									allAliveNodes.Add(temp);
								}
							}
						}
					}

					if (allAliveNodes.Any())
					{
						using (var trans = connection.BeginTransaction())
						{
							using (var commandUpdate = new SqlCommand(updateCommandText, connection, trans))
							{
								commandUpdate.Parameters.Add("@Alive", SqlDbType.Bit);
								commandUpdate.Parameters.Add("@Url", SqlDbType.NVarChar);

								foreach (var node in allAliveNodes)
								{
									var heartBeatDateTime = (DateTime)node[ordinalPosForHeartBeat];
									var url = node[ordinalPosForUrl];
									var currentDateTime = DateTime.UtcNow;
									var dateDiff = (currentDateTime - heartBeatDateTime).TotalSeconds;
									if (!(dateDiff > timeSpan.TotalSeconds)) continue;

									commandUpdate.Parameters["@Alive"].Value = false;
									commandUpdate.Parameters["@Url"].Value = url;
									commandUpdate.ExecuteNonQueryWithRetry(_retryPolicy);
									deadNodes.Add(url.ToString());
								}
							}
							trans.Commit();
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
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

					var updateCommandText = @"UPDATE [Stardust].[WorkerNode] SET Heartbeat = GETUTCDATE(),
											Alive = 1
											WHERE Url = @Url
											IF @@ROWCOUNT = 0
											BEGIN
											INSERT INTO [Stardust].[WorkerNode] (Id, Url, Heartbeat, Alive) 
											VALUES(NEWID(), @Url, GETUTCDATE(), 1)
											END";

					if (!updateStatus)
					{
						updateCommandText = @"UPDATE [Stardust].[WorkerNode] SET Heartbeat = GETUTCDATE()
											WHERE Url = @Url";
					}

					using (var command = new SqlCommand(updateCommandText, connection))
					{
						command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = nodeUri;
						command.ExecuteNonQueryWithRetry(_retryPolicy);
					}
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}
	}
}