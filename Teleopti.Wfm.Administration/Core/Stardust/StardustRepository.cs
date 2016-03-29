using System;
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
		private readonly RetryPolicy _retryPolicy;

		public StardustRepository(string connectionString)
		{
			_connectionString = connectionString;
			_retryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(maxRetry, TimeSpan.FromMilliseconds(delayMs));
		}
		

		public IList<JobHistory> HistoryList(Guid nodeId)
		{
			var selectCommand = @"SELECT * FROM [Stardust].JobHistory
								WHERE SentTo IN 
								(SELECT Url FROM [Stardust].WorkerNodes 
									WHERE Id = @NodeId) ORDER by Created desc";

			var returnList = new List<JobHistory>();

			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};
				command.Parameters.AddWithValue("@NodeId", nodeId);

				connection.OpenWithRetry(_retryPolicy);
				using (var reader = command.ExecuteReaderWithRetry(_retryPolicy))
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
				}
				connection.Close();
			}
			return returnList;
		}

		public IList<JobHistory> HistoryList()
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
				connection.OpenWithRetry(_retryPolicy);
				using (var reader = command.ExecuteReaderWithRetry(_retryPolicy))
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
				}
				connection.Close();
			}
			return returnList;
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			var selectCommand = @"SELECT  Created, Detail FROM [Stardust].JobHistoryDetail 
									WHERE JobId = @JobId ORDER BY Created desc ";

			var returnList = new List<JobHistoryDetail>();

			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};
				command.Parameters.AddWithValue("@JobId", jobId);

				connection.OpenWithRetry(_retryPolicy);
				using (var reader = command.ExecuteReaderWithRetry(_retryPolicy))
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
				}
				connection.Close();
			}
			return returnList;
		}

		public WorkerNode WorkerNode(Guid nodeId)
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
				command.Parameters.AddWithValue("@Id", nodeId);

				connection.OpenWithRetry(_retryPolicy);
				var reader = command.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						node = new WorkerNode
						{
							Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
							Url = new Uri((string)reader.GetValue(reader.GetOrdinal("Url"))),
							Alive = (bool)reader.GetValue(reader.GetOrdinal("Alive")),
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

				connection.OpenWithRetry(_retryPolicy);
				var reader = command.ExecuteReaderWithRetry(_retryPolicy);
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						var node = new WorkerNode
						{
							Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
							Url = new Uri((string)reader.GetValue(reader.GetOrdinal("Url"))),
							Alive = (bool)reader.GetValue(reader.GetOrdinal("Alive")),
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
		private string getValue<T>(object value)
		{
			return value == DBNull.Value
				? null
				: (string)value;
		}

	}
} 
	