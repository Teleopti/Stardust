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
		private DataSet _jdDataSet;
		private DataTable _jdDataTable;
		private readonly object _lockLoadAllFreeNodes = new object();
		private readonly RetryPolicyProvider _retryPolicyProvider;

		public WorkerNodeRepository(string connectionString, RetryPolicyProvider retryPolicyProvider)
		{
			_connectionString = connectionString;
			_retryPolicyProvider = retryPolicyProvider;
			InitDs();
		}

		private void Runner(Action funcToRun, string faliureMessage)
		{
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				policy.ExecuteAction(funcToRun);
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + faliureMessage);
			}
		}

		private void applyLoggingOnRetries(RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> policy)
		{
			policy.Retrying += (sender, args) =>
			{
				var msg = String.Format("Retry - Count:{0}, Delay:{1}, Exception:{2}", args.CurrentRetryCount, args.Delay,
					args.LastException);
				this.Log().ErrorWithLineNumber(msg);
			};
		}

		public List<WorkerNode> LoadAll()
		{
			var listToReturn = new List<WorkerNode>();
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				listToReturn = policy.ExecuteAction(() => tryLoadAll());
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + "Unable to add job in database");
			}

			return listToReturn;
		}

		public List<WorkerNode> tryLoadAll()
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
				connection.Open();

				var reader = command.ExecuteReader();

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
			var listToReturn = new List<WorkerNode>();
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				listToReturn = policy.ExecuteAction(() => TryLoadAllFreeNodes());
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + "Unable to add job in database");
			}

			return listToReturn;
		}

		public List<WorkerNode> TryLoadAllFreeNodes()
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
						connection.Open();
						var reader = command.ExecuteReader();
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
					this.Log().ErrorWithLineNumber("Can not get WorkerNodes",exception);
				}
				return listToReturn;
			}
		}

		public void Add(WorkerNode node)
		{
			var policy = _retryPolicyProvider.GetPolicy();
			try
			{
				policy.ExecuteAction(() => tryAdd(node));
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("UQ_WorkerNodes_Url"))
					return;
				this.Log().ErrorWithLineNumber(ex.Message + "Unable to add node in database");
			}
		}

		public void tryAdd(WorkerNode job)
		{
			var dr = _jdDataTable.NewRow();
			dr["Id"] = job.Id;
			dr["Url"] = job.Url.ToString();
			dr["Heartbeat"] = job.Heartbeat;
			dr["Alive"] = job.Alive;
			_jdDataTable.Rows.Add(dr);
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var da = new SqlDataAdapter("Select * From [Stardust].WorkerNodes",connection))
				{
					var builder = new SqlCommandBuilder(da);
					builder.GetInsertCommand();
					da.Update(_jdDataSet,"[Stardust].WorkerNodes");
				}
				connection.Close();
			}
		}

		public void DeleteNode(Guid nodeId)
		{
			Runner(() => tryDeleteNode(nodeId), "Unable to delete a node");
		}

		public void tryDeleteNode(Guid nodeId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var da = new SqlDataAdapter("Select * From [Stardust].WorkerNodes",
				                                   connection))
				{
					using (var command = new SqlCommand("DELETE FROM [Stardust].WorkerNodes WHERE Id = @ID",
					                                    connection))
					{
						var parameter = command.Parameters.Add("@ID",
						                                       SqlDbType.UniqueIdentifier,
						                                       16,
						                                       "Id");
						parameter.Value = nodeId;
						da.DeleteCommand = command;
						da.DeleteCommand.ExecuteNonQuery();
					}
				}
				connection.Close();
			}
		}

		public List<string> CheckNodesAreAlive(TimeSpan timeSpan)
		{
			var deadNodes = new List<string>();
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				deadNodes = policy.ExecuteAction(() => tryCheckNodesAreAlive(timeSpan));
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + "Unable to add job in database");
			}
			return deadNodes;
		}

		public List<string> tryCheckNodesAreAlive(TimeSpan timeSpan)
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
					connection.Open();
					int ordinalPosForHeartBeat = 0;
					int ordinalPosForUrl = 0;
					
					var listOfObjectArray = new List<object[]>();
					using (var commandSelectAll = new SqlCommand(selectCommand,connection))
					{				
						using (SqlDataReader readAllWorkerNodes =  commandSelectAll.ExecuteReader())
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
						using (var commandUpdate = new SqlCommand(updateCommandText, connection))
						{
							commandUpdate.Parameters.Add("@Alive", SqlDbType.Bit);
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
									commandUpdate.ExecuteNonQuery();
									deadNodes.Add(url.ToString());
								}
							}
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
			Runner(() => tryRegisterHeartbeat(nodeUri, updateStatus), "Unable register heartbeat");
		}

		public void tryRegisterHeartbeat(string nodeUri, bool updateStatus)
		{
			if (string.IsNullOrEmpty(nodeUri))
			{
				return;
			}

			var updateCommandText = @"UPDATE Stardust.WorkerNodes SET Heartbeat = @Heartbeat,
											Alive = @Alive
										WHERE Url = @Url";
			if(!updateStatus)
			{
				updateCommandText = @"UPDATE Stardust.WorkerNodes SET Heartbeat = @Heartbeat
										WHERE Url = @Url";
			}

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand(updateCommandText,connection))
				{
					command.Parameters.Add("@Heartbeat", SqlDbType.DateTime).Value = DateTime.Now;
					command.Parameters.Add("@Alive", SqlDbType.Bit).Value = true;
					command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = nodeUri;
					try
					{
						command.ExecuteNonQuery();
					}
					catch (Exception exp)
					{
						this.Log().ErrorWithLineNumber("Could not update heartbeat", exp);
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

		private void InitDs()
		{
			_jdDataSet = new DataSet();
			_jdDataTable = new DataTable("[Stardust].WorkerNodes");
			_jdDataTable.Columns.Add(new DataColumn("Id",typeof (Guid)));
			_jdDataTable.Columns.Add(new DataColumn("Url",typeof (string)));
			_jdDataTable.Columns.Add(new DataColumn("Heartbeat",typeof (DateTime)));
			_jdDataTable.Columns.Add(new DataColumn("Alive",typeof (bool)));
			_jdDataSet.Tables.Add(_jdDataTable);
		}
	}
}