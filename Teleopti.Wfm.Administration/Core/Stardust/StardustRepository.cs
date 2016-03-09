using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public class StardustRepository
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(StardustRepository));
		const int maxRetry = 5;
		const int delayMs = 100;

		private readonly string _connectionString;

		public StardustRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private string getValue<T>(object value)
		{
			return value == DBNull.Value
				? null
				: (string)value;
		}


		private RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> makeRetryPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(delayMs);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(maxRetry, fromMilliseconds);
			policy.Retrying += (sender, args) =>
			{
				// Log details of the retry.
				var msg = String.Format("Retry - Count:{0}, Delay:{1}, Exception:{2}", args.CurrentRetryCount, args.Delay, args.LastException);
				LogHelper.LogErrorWithLineNumber(Logger, msg);
			};
			return policy;
		}



		public JobHistory History(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				var jobHistory = readHistoryWithRetries(jobId);
				return jobHistory;
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private JobHistory readHistoryWithRetries(Guid jobId)
		{
			var policy = makeRetryPolicy();
			try
			{
				JobHistory jobHistory = null;
				policy.ExecuteAction(() => jobHistory = readHistory(jobId));
				return jobHistory;
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, "Got exception when Reading Job History", ex);
			}
			return null;
		}

		private JobHistory readHistory(Guid jobId)
		{
			var selectCommand = selectHistoryCommand(true);

			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};

				command.Parameters.Add("@JobId",
									   SqlDbType.UniqueIdentifier,
									   16,
									   "JobId");

				command.Parameters[0].Value = jobId;

				connection.Open();

				using (var reader = command.ExecuteReader())
				{
					if (reader.HasRows)
					{
						reader.Read();
						var jobHist = newJobHistoryModel(reader);

						return jobHist;
					}

					reader.Close();
					connection.Close();
				}
				return null;
			}
		}


		public IList<JobHistory> HistoryList(Guid nodeId)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				IList<JobHistory> historyList = readHistoryListWithRetries(nodeId);
				return historyList;
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private IList<JobHistory> readHistoryListWithRetries(Guid nodeId)
		{
			var policy = makeRetryPolicy();
			IList<JobHistory> jobHistory = null;
			try
			{
				policy.ExecuteAction(() => jobHistory = readHistoryList(nodeId));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, "Got exception when Reading Job History List for a node", ex);
			}
			return jobHistory;
		}

		private IList<JobHistory> readHistoryList(Guid nodeId)
		{
			var selectCommand = @"
	   SELECT * FROM [Stardust].JobHistory
	   WHERE SentTo IN 
		(SELECT Url FROM [Stardust].WorkerNodes WHERE Id = @NodeId)";

			var returnList = new List<JobHistory>();

			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};

				command.Parameters.Add("@NodeId",
										   SqlDbType.UniqueIdentifier,
										   16,
										   "NodeId");

				command.Parameters[0].Value = nodeId;

				connection.Open();

				using (var reader = command.ExecuteReader())
				{
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							var jobHist = newJobHistoryModel(reader);
							returnList.Add(jobHist);
						}
					}

					reader.Close();
					connection.Close();
				}

				return returnList;
			}
		} 



		public IList<JobHistory> HistoryList()
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				var historyList = readHistoryListWithRetries();
				return historyList;
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private IList<JobHistory> readHistoryListWithRetries()
		{
			var policy = makeRetryPolicy();
			IList<JobHistory> jobHistory = null;
			try
			{
				policy.ExecuteAction(() => jobHistory = readHistoryList());
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, "Got exception when Reading Job History List", ex);
			}
			return jobHistory;
		}

		private IList<JobHistory> readHistoryList()
		{
			var selectCommand = selectHistoryCommand(false);

			var returnList = new List<JobHistory>();

			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};

				connection.Open();

				using (var reader = command.ExecuteReader())
				{
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							var jobHist = newJobHistoryModel(reader);
							returnList.Add(jobHist);
						}
					}

					reader.Close();
					connection.Close();
				}

				return returnList;
			}
		} 

		private JobHistory newJobHistoryModel(SqlDataReader reader)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				var jobHist = new JobHistory
				{
					Id = (Guid)reader.GetValue(reader.GetOrdinal("JobId")),
					Name = (string)reader.GetValue(reader.GetOrdinal("Name")),
					CreatedBy = (string)reader.GetValue(reader.GetOrdinal("CreatedBy")),
					SentTo = getValue<string>(reader.GetValue(reader.GetOrdinal("SentTo"))),
					Result = getValue<string>(reader.GetValue(reader.GetOrdinal("Result"))),
					Created = (DateTime)(reader.GetValue(reader.GetOrdinal("Created"))),
					Started = getDateTime(reader.GetValue(reader.GetOrdinal("Started"))),
					Ended = getDateTime(reader.GetValue(reader.GetOrdinal("Ended")))
				};

				return jobHist;
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private static string selectHistoryCommand(bool byJobId)
		{
			var selectCommand = @"SELECT 
                                             JobId    
                                            ,Name
                                            ,CreatedBy
                                            ,Created
                                            ,Started
                                            ,Ended
                                            ,SentTo,
															Result
                                        FROM [Stardust].JobHistory";


			if (byJobId)
			{
				selectCommand += " WHERE JobId = @JobId";
			}

			return selectCommand;
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				IList<JobHistoryDetail> jobHistoryDetail = readJobHistoryDetailsWithRetries(jobId);
				return jobHistoryDetail;
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private IList<JobHistoryDetail> readJobHistoryDetailsWithRetries(Guid jobId)
		{
			var policy = makeRetryPolicy();
			IList<JobHistoryDetail> jobHistory = null;
			try
			{
				policy.ExecuteAction(() => jobHistory = readJobHistryDetails(jobId));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, "Got exception when Reading Job History List for a node", ex);
			}
			return jobHistory;
		}

		private IList<JobHistoryDetail> readJobHistryDetails(Guid jobId)
		{
			var selectCommand = @"SELECT  Created, Detail  FROM [Stardust].JobHistoryDetail WHERE JobId = @JobId";

			var returnList = new List<JobHistoryDetail>();

			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};
				command.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier, 16, "JobId");
				command.Parameters[0].Value = jobId;
				connection.Open();

				using (var reader = command.ExecuteReader())
				{
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							var detail = new JobHistoryDetail
							{
								Created = (DateTime)(reader.GetValue(reader.GetOrdinal("Created"))),
								Detail = (string)(reader.GetValue(reader.GetOrdinal("Detail"))),
							};
							returnList.Add(detail);
						}
					}
					reader.Close();
					connection.Close();
				}

				return returnList;
			}
		} 

		private DateTime? getDateTime(object databaseValue)
		{
			if (databaseValue.Equals(DBNull.Value))
			{
				return null;
			}

			return (DateTime)databaseValue;
		}

		public WorkerNode WorkerNode(Guid nodeId)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				var node = readWorkerNodeWithRetries(nodeId);
				return node;
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private WorkerNode readWorkerNodeWithRetries(Guid nodeId)
		{
			var policy = makeRetryPolicy();
			WorkerNode workerNode = null;
			try
			{
				policy.ExecuteAction(() => workerNode = readWorkerNode(nodeId));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, "Got exception when Reading Job History List for a node", ex);
			}
			return workerNode;
		}


		private WorkerNode readWorkerNode(Guid nodeId)
		{
			const string selectCommand = @"SELECT w.Id, Url, Heartbeat, Alive, CASE 
							WHEN AssignedNode IS NULL 
							THEN CONVERT(bit,0) ELSE CONVERT(bit,1) END AS Running  
							FROM [Stardust].WorkerNodes w 
							LEFT JOIN [Stardust].jobDefinitions j ON w.Url=j.AssignedNode
							WHERE w.Id = @Id";

			WorkerNode node = null;
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};
				command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier, 16, "Id");
				command.Parameters[0].Value = nodeId;
				connection.Open();

				var reader = command.ExecuteReader();

				if (reader.HasRows)
				{
					while (reader.Read())
					{
						node = new WorkerNode
						{
							Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
							Url = new Uri((string)reader.GetValue(reader.GetOrdinal("Url"))),
							Alive = (string)reader.GetValue(reader.GetOrdinal("Alive")),
							Heartbeat = (DateTime)reader.GetValue(reader.GetOrdinal("Heartbeat")),
							Running = (bool)reader.GetValue(reader.GetOrdinal("Running"))
						};
					}
				}

				reader.Close();
				connection.Close();
			}
			return node;
		}


		public List<WorkerNode> WorkerNodes()
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				var nodes = readWorkerNodesWithRetries();
				return nodes;
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private List<WorkerNode> readWorkerNodesWithRetries()
		{
			var policy = makeRetryPolicy();
			List<WorkerNode> workerNodes = null;
			try
			{
				policy.ExecuteAction(() => workerNodes = readWorkerNodes());
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, "Got exception when Reading Job History List for a node", ex);
			}
			return workerNodes;
		}

		private List<WorkerNode> readWorkerNodes()
		{
			const string selectCommand = @"SELECT w.Id, Url, Heartbeat, Alive, CASE 
							WHEN AssignedNode IS NULL 
							THEN CONVERT(bit,0) ELSE CONVERT(bit,1) END AS Running  
							FROM [Stardust].WorkerNodes w 
							LEFT JOIN [Stardust].jobDefinitions j ON w.Url=j.AssignedNode";

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
						var node = new WorkerNode
						{
							Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
							Url = new Uri((string)reader.GetValue(reader.GetOrdinal("Url"))),
							Alive = (string)reader.GetValue(reader.GetOrdinal("Alive")),
							Heartbeat = (DateTime)reader.GetValue(reader.GetOrdinal("Heartbeat")),
							Running = (bool)reader.GetValue(reader.GetOrdinal("Running"))
						};

						listToReturn.Add(node);
					}
				}

				reader.Close();
				connection.Close();
			}
			return listToReturn;
		}
	}
} 
	