using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public class StardustRepository
	{
		const int maxRetry = 5;
		const int delayMs = 100;

		private readonly string _connectionString;
		private readonly RetryPolicy _retryPolicy;

		public StardustRepository(string connectionString)
		{
			_connectionString = connectionString;
			_retryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(maxRetry, TimeSpan.FromMilliseconds(delayMs));
		}
		
		public IList<Job> GetJobsByNodeId(Guid nodeId)
		{
			var selectCommandText = @"SELECT * FROM [Stardust].Job
								WHERE SentToWorkerNodeUri IN 
								(SELECT Url FROM [Stardust].WorkerNode 
									WHERE Id = @NodeId) ORDER by Created desc";

			var returnList = new List<Job>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);
				using (var selectCommand = new SqlCommand(selectCommandText, connection))
				{
					selectCommand.Parameters.AddWithValue("@NodeId", nodeId);
					using (var reader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (reader.HasRows)
						{
							while (reader.Read())
							{
								var job = createJobFromSqlDataReader(reader);
								returnList.Add(job);
							}
						}
					}
				}
				connection.Close();
			}
			return returnList;
		}


		public IList<Job> GetAllJobs()
		{
			var jobs = new List<Job>();
			
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					var selectCommandText = @"SELECT  [JobId]
											  ,[Name]
											  ,[Created]
											  ,[CreatedBy]
											  ,[Started]
											  ,[Ended]
											  ,[Serialized]
											  ,[Type]
											  ,[SentToWorkerNodeUri]
											  ,[Result]
										  FROM [Stardust].[Job] order by Created desc";
				using (var getAllJobsCommand = new SqlCommand(selectCommandText,sqlConnection))
					{
					using (var sqlDataReader = getAllJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (sqlDataReader.HasRows)
							{
								while (sqlDataReader.Read())
								{
									var job = createJobFromSqlDataReader(sqlDataReader);
									jobs.Add(job);
								}
							}
						}
					}
				}
			return jobs;
		}

		public Job GetJobByJobId(Guid jobId)
		{
			var job = new Job();

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				var selectCommandText = @"SELECT  [JobId]
											  ,[Name]
											  ,[Created]
											  ,[CreatedBy]
											  ,[Started]
											  ,[Ended]
											  ,[Serialized]
											  ,[Type]
											  ,[SentToWorkerNodeUri]
											  ,[Result]
										  FROM [Stardust].[Job] WHERE JobId = @jobId";
				using (var selectCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					selectCommand.Parameters.AddWithValue("@JobId", jobId);
					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							sqlDataReader.Read();

							job = createJobFromSqlDataReader(sqlDataReader);
						}
					}
				}
			}
			return job;
		}

		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			var jobDetails = new List<JobDetail>();

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				var selectCommandText = @"SELECT  
											Id, 
											JobId, 
											Created, 
											Detail  
										FROM [Stardust].[JobDetail] WITH (NOLOCK) 
										WHERE JobId = @JobId ORDER BY Created desc";
				using (var selectCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					selectCommand.Parameters.AddWithValue("@JobId", jobId);
					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							while (sqlDataReader.Read())
							{
								var jobDetail = CreateJobDetailFromSqlDataReader(sqlDataReader);
								jobDetails.Add(jobDetail);
							}
						}
					}
				}
				return jobDetails;
			}
		}

		public WorkerNode WorkerNode(Guid nodeId)
		{
			const string selectCommandText = @"SELECT DISTINCT Id, Url, Heartbeat, Alive, Running
											FROM (SELECT Id, Url, Heartbeat, Alive, CASE WHEN Url IN 
											(SELECT SentToWorkerNodeUri FROM Stardust.Job WHERE Ended IS NULL) THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS Running 
											FROM [Stardust].WorkerNode, [Stardust].job) w
											WHERE w.Id = @Id";
			WorkerNode node = null;
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);
				using (var selectCommand = new SqlCommand(selectCommandText, connection))
				{
					selectCommand.Parameters.AddWithValue("@Id", nodeId);
					var reader = selectCommand.ExecuteReader();
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							node = new WorkerNode
							{
								Id = reader.GetGuid(reader.GetOrdinal("Id")),
								Url = new Uri(reader.GetString(reader.GetOrdinal("Url"))),
								Alive = reader.GetBoolean(reader.GetOrdinal("Alive")),
								Heartbeat = reader.GetDateTime(reader.GetOrdinal("Heartbeat")),
								Running = reader.GetBoolean(reader.GetOrdinal("Running"))
							};
						}
					}
					connection.Close();
				}
			}
			return node;
		}
		
		public List<WorkerNode> GetAllWorkerNodes()
		{
			var listToReturn = new List<WorkerNode>();
			var commandText = @"SELECT DISTINCT Id, Url, Heartbeat, Alive, Running
							FROM (SELECT Id, Url, Heartbeat, Alive, CASE WHEN Url IN 
							(SELECT SentToWorkerNodeUri FROM Stardust.Job WHERE Ended IS NULL) THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS Running 
							FROM [Stardust].WorkerNode, [Stardust].job) w";
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);

				using (var selectCommand = new SqlCommand(commandText, connection))
				{
					using (var reader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (reader.HasRows)
						{
							var ordinalPositionForIdField = reader.GetOrdinal("Id");
							var ordinalPositionForUrlField = reader.GetOrdinal("Url");
							var ordinalPositionForAliveField = reader.GetOrdinal("Alive");
							var ordinalPositionForRunningField = reader.GetOrdinal("Running");
							var ordinalPositionForHeartbeatField = reader.GetOrdinal("Heartbeat");

							while (reader.Read())
							{
								var workerNode = new WorkerNode
								{
									Id = (Guid) reader.GetValue(ordinalPositionForIdField),
									Url = new Uri((string) reader.GetValue(ordinalPositionForUrlField)),
									Alive = (bool) reader.GetValue(ordinalPositionForAliveField),
									Heartbeat = (DateTime) reader.GetValue(ordinalPositionForHeartbeatField),
									Running = (bool)reader.GetValue(ordinalPositionForRunningField),
								};
								listToReturn.Add(workerNode);
							}
						}
					}
				}
			}
			return listToReturn;
		}


		private T getDBNullSafeValue<T>(object value) where T : class
		{
			return value == DBNull.Value ? null : (T)value;
		}
		

		private Job createJobFromSqlDataReader(SqlDataReader sqlDataReader)
		{
			var job = new Job
			{
				JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
				Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name")),
				SentToWorkerNodeUri = getDBNullSafeValue<string>(sqlDataReader.GetValue(sqlDataReader.GetOrdinal("SentToWorkerNodeUri"))),
				Type = getDBNullSafeValue<string>(sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Type"))),
				CreatedBy = sqlDataReader.GetString(sqlDataReader.GetOrdinal("CreatedBy")),
				Result = getDBNullSafeValue<string>(sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Result"))),
				Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created")),
				Serialized = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Serialized")),
				Started = sqlDataReader.GetNullableDateTime(sqlDataReader.GetOrdinal("Started")),
				Ended = sqlDataReader.GetNullableDateTime(sqlDataReader.GetOrdinal("Ended"))
			};
			return job;
		}

		private JobDetail CreateJobDetailFromSqlDataReader(SqlDataReader sqlDataReader)
		{
			var jobDetail = new JobDetail
			{
				Id = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Id")),
				JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
				Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created")),
				Detail = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Detail"))
			};
			return jobDetail;
		}
	}

	public static class SqlDataReaderExtensions
	{
		public static DateTime? GetNullableDateTime(this SqlDataReader reader, int ordinalPosition)
		{
			return reader.IsDBNull(ordinalPosition) ? (DateTime?)null : reader.GetDateTime(ordinalPosition);
		}

		public static string GetNullableString(this SqlDataReader reader, int ordinalPosition)
		{
			if (reader.IsDBNull(ordinalPosition))
			{
				return null;
			}
			return reader.GetString(ordinalPosition);
		}
	}
} 
	