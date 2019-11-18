using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Polly.Retry;
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
                    _retryPolicy.Execute(connection.Open);

					using (var workerNodeCommand = new SqlCommand(selectWorkerNodeCommand, connection))
					{
						workerNodeCommand.Parameters.AddWithValue("@Id", workerNode.Id);
						workerNodeCommand.Parameters.AddWithValue("@Url", workerNode.Url.ToString());
						workerNodeCommand.Parameters.AddWithValue("@Heartbeat", workerNode.Heartbeat);
						workerNodeCommand.Parameters.AddWithValue("@Alive", workerNode.Alive);

                        _retryPolicy.Execute(workerNodeCommand.ExecuteNonQuery);
					}
				}
			}
			catch (Exception exp)
			{
				if (exp.Message.Contains("UQ_WorkerNodes_Url"))
				{
					using (var connection = new SqlConnection(_connectionString))
					{
                        _retryPolicy.Execute(connection.Open);
						const string updateCommandText = @"UPDATE [Stardust].[WorkerNode] SET Heartbeat = @Heartbeat,
											Alive = @Alive
											WHERE Url = @Url";

						using (var command = new SqlCommand(updateCommandText, connection))
						{
							command.Parameters.Add("@Heartbeat", SqlDbType.DateTime).Value = DateTime.UtcNow;
							command.Parameters.Add("@Alive", SqlDbType.Bit).Value = true;
							command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = workerNode.Url.ToString();

                            _retryPolicy.Execute(command.ExecuteNonQuery);
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
                    _retryPolicy.Execute(connection.Open);

					var allAliveNodes = new List<Tuple<DateTime,string>>();
					using (var commandSelectAll = new SqlCommand(selectCommand, connection))
					{
						using (var readAllWorkerNodes = _retryPolicy.Execute(commandSelectAll.ExecuteReader))
						{
							if (readAllWorkerNodes.HasRows)
							{
								var ordinalPosForHeartBeat = readAllWorkerNodes.GetOrdinal("Heartbeat");
								var ordinalPosForUrl = readAllWorkerNodes.GetOrdinal("Url");

								while (readAllWorkerNodes.Read())
								{
									allAliveNodes.Add(new Tuple<DateTime, string>(readAllWorkerNodes.GetDateTime(ordinalPosForHeartBeat),
										readAllWorkerNodes.GetString(ordinalPosForUrl)));
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
									var heartBeatDateTime = node.Item1;
									var url = node.Item2;
									var currentDateTime = DateTime.UtcNow;
									var dateDiff = (currentDateTime - heartBeatDateTime).TotalSeconds;
									if (dateDiff <= timeSpan.TotalSeconds) continue;

									commandUpdate.Parameters["@Alive"].Value = false;
									commandUpdate.Parameters["@Url"].Value = url;
                                    _retryPolicy.Execute(commandUpdate.ExecuteNonQuery);
									deadNodes.Add(url);
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
                    _retryPolicy.Execute(connection.Open);

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
                        _retryPolicy.Execute(command.ExecuteNonQuery);
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