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
		private readonly object _lockLoadAllFreeNodes = new object();
		private readonly RetryPolicy _retryPolicy;

		public WorkerNodeRepository(string connectionString, RetryPolicyProvider retryPolicyProvider)
		{
			_connectionString = connectionString;
			_retryPolicy = retryPolicyProvider.GetPolicy();
		}

		public List<WorkerNode> LoadAll()
		{
			const string selectCommand = @"SELECT * FROM [Stardust].WorkerNodes";
			var listToReturn = new List<WorkerNode>();
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};
				connection.OpenWithRetry(_retryPolicy);

				var reader = command.ExecuteReaderWithRetry(_retryPolicy);

				if (reader.HasRows)
				{
					while (reader.Read())
					{
						var jobDefinition = new WorkerNode
						{
							Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
							Url = new Uri((string)reader.GetValue(reader.GetOrdinal("Url"))),
							Alive = (bool)reader.GetValue(reader.GetOrdinal("Alive")),
							Heartbeat = (DateTime)reader.GetValue(reader.GetOrdinal("Heartbeat"))
						};
						listToReturn.Add(jobDefinition);
					}
				}

				reader.Close();
				connection.Close();
			}
			return listToReturn;
		}

		public List<WorkerNode> LoadAllFreeNodes()
		{
			lock (_lockLoadAllFreeNodes)
			{
				const string selectCommand =
					@"SELECT * FROM [Stardust].WorkerNodes WHERE Url NOT IN (SELECT ISNULL(AssignedNode,'') FROM [Stardust].JobDefinitions)";

				var listToReturn = new List<WorkerNode>();
				try
				{
					using (var connection = new SqlConnection(_connectionString))
					{
						var command = new SqlCommand
						{
							Connection = connection,
							CommandText = selectCommand,
							CommandType = CommandType.Text
						};
						connection.OpenWithRetry(_retryPolicy);
						var reader = command.ExecuteReaderWithRetry(_retryPolicy);
						if (reader.HasRows)
						{
							while (reader.Read())
							{
								var jobDefinition = new WorkerNode
								{
									Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
									Url = new Uri((string)reader.GetValue(reader.GetOrdinal("Url"))),
									Alive = (bool)reader.GetValue(reader.GetOrdinal("Alive")),
									Heartbeat = (DateTime)reader.GetValue(reader.GetOrdinal("Heartbeat"))
								};
								listToReturn.Add(jobDefinition);
							}
						}
						reader.Close();
						connection.Close();
					}
				}
				catch (TimeoutException exception)
				{
					this.Log().ErrorWithLineNumber("Can not get WorkerNodes, maybe there is a lock in Stardust.JobDefinitions table",
													 exception);
				}
				catch (Exception exception)
				{
					this.Log().ErrorWithLineNumber("Can not get WorkerNodes", exception);
				}
				return listToReturn;
			}
		}

		public void Add(WorkerNode job)
		{

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);
				SqlCommand workerNodeCommand = connection.CreateCommand();
				workerNodeCommand.CommandText = "INSERT INTO [Stardust].WorkerNodes (Id, Url, Heartbeat, Alive) VALUES(@Id, @Url, @Heartbeat, @Alive)";
				workerNodeCommand.Parameters.AddWithValue("@Id", job.Id);
				workerNodeCommand.Parameters.AddWithValue("@Url", job.Url.ToString());
				workerNodeCommand.Parameters.AddWithValue("@Heartbeat", job.Heartbeat);
				workerNodeCommand.Parameters.AddWithValue("@Alive", job.Alive);
				try
				{
					using (var tran = connection.BeginTransaction())
					{
						workerNodeCommand.Transaction = tran;
						workerNodeCommand.ExecuteNonQueryWithRetry(_retryPolicy);
						tran.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					connection.Close();
				}
			}
		}

		public void DeleteNode(Guid nodeId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				SqlCommand deleteCommand = connection.CreateCommand();
				deleteCommand.CommandText = "DELETE FROM[Stardust].WorkerNodes WHERE Id = @ID";
				deleteCommand.Parameters.AddWithValue("@ID", nodeId);

				try
				{
					connection.OpenWithRetry(_retryPolicy);
					using (var tran = connection.BeginTransaction())
					{
						deleteCommand.Transaction = tran;
						deleteCommand.ExecuteNonQueryWithRetry(_retryPolicy);
						tran.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					connection.Close();
				}
			}
			
		}

		
		public List<string> CheckNodesAreAlive(TimeSpan timeSpan)
		{
			var selectCommand = @"SELECT Id, Url, Heartbeat, Alive FROM Stardust.WorkerNodes";
			var updateCommandText = @"UPDATE Stardust.WorkerNodes 
											SET Alive = @Alive
										WHERE Url = @Url";
			var deadNodes = new List<string>();
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);
					int ordinalPosForHeartBeat = 0;
					int ordinalPosForUrl = 0;

					var listOfObjectArray = new List<object[]>();
					using (var commandSelectAll = new SqlCommand(selectCommand, connection))
					{
						using (SqlDataReader readAllWorkerNodes = commandSelectAll.ExecuteReaderWithRetry(_retryPolicy))
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
							using (var commandUpdate = new SqlCommand(updateCommandText, connection,trans))
							{
								commandUpdate.Parameters.Add("@Alive", SqlDbType.NVarChar);
								commandUpdate.Parameters.Add("@Url", SqlDbType.NVarChar);

								foreach (var objectse in listOfObjectArray)
								{
									var heartBeatDateTime =
										(DateTime)objectse[ordinalPosForHeartBeat];
									var url = objectse[ordinalPosForUrl];
									var currentDateTime = DateTime.Now;
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
				this.Log().ErrorWithLineNumber(exp.Message,
															exp);
				throw;
			}

			return deadNodes;
		}

		public void RegisterHeartbeat(string nodeUri, bool updateStatus)
		{
			if (string.IsNullOrEmpty(nodeUri))
			{
				return;
			}

			var updateCommandText = @"UPDATE Stardust.WorkerNodes SET Heartbeat = @Heartbeat,
											Alive = @Alive
										WHERE Url = @Url";
			if (!updateStatus)
			{
				updateCommandText = @"UPDATE Stardust.WorkerNodes SET Heartbeat = @Heartbeat
										WHERE Url = @Url";
			}

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);
				using (var trans = connection.BeginTransaction())
				{
					using (var command = new SqlCommand(updateCommandText, connection,trans))
					{
						command.Parameters.Add("@Heartbeat", SqlDbType.DateTime).Value = DateTime.Now;
						command.Parameters.Add("@Alive", SqlDbType.Bit).Value = true;
						command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = nodeUri;
						try
						{
							command.ExecuteNonQueryWithRetry(_retryPolicy);
							trans.Commit();
						}
						catch (Exception exp)
						{
							this.Log().ErrorWithLineNumber("Could not update heartbeat", exp);
						}
					}
				}
				
				connection.Close();
			}
		}

		public WorkerNode LoadWorkerNode(Uri nodeUri)
		{
			var workerNodes = LoadAll();
			return workerNodes.FirstOrDefault(node => node.Url == nodeUri);
		}
	}
}