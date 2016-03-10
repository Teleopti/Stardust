using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class WorkerNodeRepository : IWorkerNodeRepository
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (WorkerNodeRepository));

		private readonly string _connectionString;
		private DataSet _jdDataSet;
		private DataTable _jdDataTable;

		public WorkerNodeRepository(string connectionString)
		{
			_connectionString = connectionString;

			InitDs();
		}

		public List<WorkerNode> LoadAll()
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start LoadAll.");

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
							Id = (Guid) reader.GetValue(reader.GetOrdinal("Id")),
							Url = new Uri((string) reader.GetValue(reader.GetOrdinal("Url"))),
							Alive = (string) reader.GetValue(reader.GetOrdinal("Alive")),
							Heartbeat = (DateTime) reader.GetValue(reader.GetOrdinal("Heartbeat"))
						};

						listToReturn.Add(jobDefinition);
					}
				}

				reader.Close();
				connection.Close();
			}

			if (listToReturn.Any())
			{
				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Found ( " + listToReturn.Count + " ) availabe nodes.");
			}
			else
			{
				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "No nodes found.");
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished LoadAll.");

			return listToReturn;
		}

		private readonly object _lockLoadAllFreeNodes=new object();

		public List<WorkerNode> LoadAllFreeNodes()
		{
			lock (_lockLoadAllFreeNodes)
			{
				LogHelper.LogDebugWithLineNumber(Logger, "Start LoadAllFreeNodes.");

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
									Alive = (string)reader.GetValue(reader.GetOrdinal("Alive")),
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
					LogHelper.LogErrorWithLineNumber(Logger,
													 "Can not get WorkerNodes, maybe there is a lock in Stardust.JobDefinitions table",
													 exception);
				}

				catch (Exception exception)
				{
					LogHelper.LogErrorWithLineNumber(Logger,
													 "Can not get WorkerNodes",
													 exception);
				}


				if (listToReturn.Any())
				{
					LogHelper.LogDebugWithLineNumber(Logger, "Found ( " + listToReturn.Count + " ) availabe nodes.");
				}
				else
				{
					LogHelper.LogDebugWithLineNumber(Logger, "No nodes found.");
				}


				LogHelper.LogDebugWithLineNumber(Logger, "Finished LoadAllFreeNodes.");

				return listToReturn;

			}
		}

		public void Add(WorkerNode job)
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

				using (var da = new SqlDataAdapter("Select * From [Stardust].WorkerNodes",
				                                   connection))
				{
					var builder = new SqlCommandBuilder(da);

					builder.GetInsertCommand();

					da.Update(_jdDataSet,
					          "[Stardust].WorkerNodes");
				}

				connection.Close();
			}
		}

		public void DeleteNode(Guid nodeId)
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
			var selectCommand = @"SELECT Id, Url, Heartbeat, Alive 
									 FROM Stardust.WorkerNodes";

			var updateCommandText = @"UPDATE Stardust.WorkerNodes 
											SET Alive = @Alive
										WHERE Url = @Url";

			LogHelper.LogDebugWithLineNumber(Logger, "Start");

			var deadNodes = new List<string>();

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					int ordinalPosForHeartBeat = 0;
					int ordinalPosForUrl = 0;

					List<object[]> listOfObjectArray = new List<object[]>();

					using (var commandSelectAll = new SqlCommand(selectCommand,
					                                             connection))
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

									int instances = readAllWorkerNodes.GetValues(temp);

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
									var alive = "false";

									commandUpdate.Parameters["@Alive"].Value = alive;
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
				LogHelper.LogErrorWithLineNumber(Logger,
				                                 exp.Message,
				                                 exp);
				throw;
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished");

			return deadNodes;
		}

		public void RegisterHeartbeat(string nodeUri, bool updateStatus)
		{
			// Validate argument.
			if (string.IsNullOrEmpty(nodeUri))
			{
				return;
			}

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "Start register heartbeat for url : " + nodeUri);
			
			// Update row.
				var updateCommandText = @"UPDATE Stardust.WorkerNodes 
										SET Heartbeat = @Heartbeat,
											Alive = @Alive
										WHERE Url = @Url";

			
			if(!updateStatus)
			{
				updateCommandText = @"UPDATE Stardust.WorkerNodes 
										SET Heartbeat = @Heartbeat
										WHERE Url = @Url";
			}

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				using (var command = new SqlCommand(updateCommandText,
				                                    connection))
				{
					command.Parameters.Add("@Heartbeat",
					                       SqlDbType.DateTime).Value = DateTime.Now;

					
						command.Parameters.Add("@Alive",
										  SqlDbType.NVarChar).Value = "true";
					
					
					command.Parameters.Add("@Url",
					                       SqlDbType.NVarChar).Value = nodeUri;
					try
					{
						command.ExecuteNonQuery();
					}
					catch (Exception exp)
					{
						LogHelper.LogErrorWithLineNumber(Logger, "Could not update heartbeat", exp);
					}
				}

				connection.Close();
			}
		}

		public WorkerNode LoadWorkerNode(Uri nodeUri)
		{
			var workerNodes = LoadAll();
			foreach (var node in workerNodes)
			{
				if (node.Url == nodeUri)
				{
					return node;
				}
			}
			return null;
		}

		private void InitDs()
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start InitDs.");

			_jdDataSet = new DataSet();

			_jdDataTable = new DataTable("[Stardust].WorkerNodes");

			_jdDataTable.Columns.Add(new DataColumn("Id",
			                                        typeof (Guid)));

			_jdDataTable.Columns.Add(new DataColumn("Url",
			                                        typeof (string)));

			_jdDataTable.Columns.Add(new DataColumn("Heartbeat",
			                                        typeof (DateTime)));

			_jdDataTable.Columns.Add(new DataColumn("Alive",
			                                        typeof (string)));

			_jdDataSet.Tables.Add(_jdDataTable);

			LogHelper.LogDebugWithLineNumber(Logger, "Finished InitDs.");
		}
	}
}