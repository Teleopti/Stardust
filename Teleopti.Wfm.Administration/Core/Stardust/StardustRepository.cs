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

		private void runner(Action funcToRun, string faliureMessage)
		{
			var policy = makeRetryPolicy();
			try
			{
				policy.ExecuteAction(funcToRun);
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + faliureMessage);
			}
		}

		public IList<JobHistory> HistoryList(Guid nodeId)
		{
			IList<JobHistory> historyList = null;

			runner(() => historyList = readHistoryList(nodeId), "Unable to read History List for a specific node. nodeId: " + nodeId);

			return historyList;
		}

		public IList<JobHistory> HistoryList()
		{
			IList<JobHistory> historyList = null;

			runner(() => historyList = readHistoryList(), "Unable to read History List");

			return historyList;
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			IList<JobHistoryDetail> jobHistoryDetails = null;

			runner(() => jobHistoryDetails = readJobHistryDetails(jobId), "Unable to read History Details for jobId " + jobId);
			
			return jobHistoryDetails;
		}

		public WorkerNode WorkerNode(Guid nodeId)
		{
			WorkerNode workerNode = null;

			runner(() => workerNode = readWorkerNode(nodeId), "Unable to read WorkerNode for nodeId " + nodeId);

			return workerNode;
		}

		public List<WorkerNode> WorkerNodes()
		{
			List<WorkerNode> workerNodes = null;

			runner(() => workerNodes = readWorkerNodes(), "Unable to read WorkerNodes");

			return workerNodes;
		}


		private IList<JobHistory> readHistoryList(Guid nodeId)
		{
			var selectCommand = @"
	   SELECT * FROM [Stardust].JobHistory
	   WHERE SentTo IN 
		(SELECT Url FROM [Stardust].WorkerNodes WHERE Id = @NodeId) ORDER by Created desc";

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
		
		private IList<JobHistory> readHistoryList()
		{
			var selectCommand = @"SELECT	JobId
											, Name
											, CreatedBy
											, Created
											, Started
											, Ended
											, SentTo
											, Result
										FROM[Stardust].JobHistory ORDER BY Created desc";

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

		private IList<JobHistoryDetail> readJobHistryDetails(Guid jobId)
		{
			var selectCommand = @"SELECT  Created, Detail FROM [Stardust].JobHistoryDetail WHERE JobId = @JobId ORDER BY Created desc ";

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
		

		private DateTime? getDateTime(object databaseValue)
		{
			if (databaseValue.Equals(DBNull.Value))
			{
				return null;
			}

			return (DateTime)databaseValue;
		}
		
	}
} 
	