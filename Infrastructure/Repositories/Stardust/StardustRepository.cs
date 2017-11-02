﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Infrastructure.Repositories.Stardust
{
	public class StardustRepository : IStardustRepository
	{
		private const int maxRetry = 5;
		private const int delayMs = 100;

		private readonly string _connectionString;
		private readonly RetryPolicy _retryPolicy;

		public StardustRepository(string connectionString)
		{
			_connectionString = connectionString;
			_retryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(maxRetry, TimeSpan.FromMilliseconds(delayMs));
		}

		public IList<Job> GetJobsByNodeId(Guid nodeId, int from, int to)
		{
			var jobs = new List<Job>();

			using (var connection = new SqlConnection(_connectionString))
			{
				const string selectCommandText = @"WITH Ass AS (SELECT top (1000000) *, ROW_NUMBER() OVER (ORDER BY Started desc) AS 'RowNumber' FROM (SELECT * FROM [Stardust].Job
								WHERE SentToWorkerNodeUri IN (SELECT Url FROM [Stardust].WorkerNode WHERE Id = @nodeId)) as b ORDER BY Started desc ) 
								SELECT * FROM Ass WITH(NOLOCK) WHERE RowNumber BETWEEN @from AND @to";

				connection.OpenWithRetry(_retryPolicy);
				using (var selectCommand = new SqlCommand(selectCommandText, connection))
				{
					selectCommand.Parameters.AddWithValue("@nodeId", nodeId);
					selectCommand.Parameters.AddWithValue("@from", from);
					selectCommand.Parameters.AddWithValue("@to", to);
					using (var reader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!reader.HasRows) return jobs;
						while (reader.Read())
						{
							var job = createJobFromSqlDataReader(reader);
							setTotalDuration(job);
							jobs.Add(job);
						}
					}
				}
			}
			return jobs;
		}

		private static void setTotalDuration(Job job)
		{
			var totalDuration = (DateTime.UtcNow - job.Started).ToString();
			if (!string.IsNullOrEmpty(job.Ended.ToString()))
				totalDuration = (job.Ended - job.Started).ToString();
			job.TotalDuration = totalDuration.Substring(0, 8);
		}


		public IList<Job> GetAllJobs(int from, int to)
		{
			var jobs = new List<Job>();

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				const string selectCommandText = @"WITH Ass AS (SELECT top (1000000) *,  ROW_NUMBER() OVER (ORDER BY Created desc) AS 'RowNumber' 
											FROM Stardust.Job WITH(NOLOCK) ORDER BY Created desc ) SELECT * FROM Ass WITH(NOLOCK) WHERE RowNumber BETWEEN @from AND @to";
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getAllJobsCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					getAllJobsCommand.Parameters.AddWithValue("@from", from);
					getAllJobsCommand.Parameters.AddWithValue("@to", to);
					using (var reader = getAllJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!reader.HasRows) return jobs;
						while (reader.Read())
						{
							var job = createJobFromSqlDataReader(reader);
							setTotalDuration(job);
							jobs.Add(job);
						}
					}
				}
			}
			return jobs;
		}

		public List<string> GetAllTypes()
		{
			const string selectCommandText = "SELECT DISTINCT Type FROM Stardust.Job WITH(NOLOCK)";
			var types = new List<string>();
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var selectTypesCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var sqlDataReader = selectTypesCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return types;
						while (sqlDataReader.Read())
						{
							types.Add(sqlDataReader.GetString(0).Substring(44));
						}
					}
				}
			}
			return types;
		}

		public IList<Job> GetAllJobs(JobFilterModel filter)
		{
			var jobs = new List<Job>();

			var selectCommandText =
				@"WITH Ass AS (SELECT top (1000000) *,  ROW_NUMBER() OVER (ORDER BY Created desc) AS 'RowNumber' 
				FROM Stardust.Job WITH(NOLOCK) ORDER BY Created desc ) SELECT * FROM Ass WITH(NOLOCK) WHERE RowNumber BETWEEN @from AND @to ";

			if (filter.DataSource != null)
				selectCommandText = selectCommandText + $@"AND Serialized LIKE '%LogOnDatasource"":""{filter.DataSource}%' ";

			if(filter.Type != null)
				selectCommandText = selectCommandText + $@"AND Type = 'Teleopti.Ccc.Domain.ApplicationLayer.Events.{filter.Type}' ";

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getAllJobsCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					getAllJobsCommand.Parameters.AddWithValue("@from", filter.From);
					getAllJobsCommand.Parameters.AddWithValue("@to", filter.To);
					using (var reader = getAllJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!reader.HasRows) return jobs;
						while (reader.Read())
						{
							var job = createJobFromSqlDataReader(reader);
							setTotalDuration(job);
							jobs.Add(job);
						}
					}
				}
			}
			return jobs;
		}

		public IList<Job> GetAllQueuedJobs(JobFilterModel filter)
		{
			var jobs = new List<Job>();

			var selectCommandText =
				@"WITH Ass AS (SELECT top (1000000) *,  ROW_NUMBER() OVER (ORDER BY Created desc) AS 'RowNumber' 
				FROM Stardust.JobQueue WITH(NOLOCK) ORDER BY Created desc ) SELECT * FROM Ass WITH(NOLOCK) WHERE RowNumber BETWEEN @from AND @to ";

			if (filter.DataSource != null)
				selectCommandText = selectCommandText + $@"AND Serialized LIKE '%LogOnDatasource"":""{filter.DataSource}%' ";

			if (filter.Type != null)
				selectCommandText = selectCommandText + $@"AND Type = 'Teleopti.Ccc.Domain.ApplicationLayer.Events.{filter.Type}' ";

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getQueuedJobsCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					getQueuedJobsCommand.Parameters.AddWithValue("@from", filter.From);
					getQueuedJobsCommand.Parameters.AddWithValue("@to", filter.To);
					using (var reader = getQueuedJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!reader.HasRows) return jobs;
						while (reader.Read())
						{
							var job = createQueuedJobFromSqlDataReader(reader);
							jobs.Add(job);
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
				const string selectCommandText = @"SELECT  [JobId] ,[Name] ,[Created] ,[CreatedBy] ,[Started] ,[Ended] ,[Serialized] ,[Type] ,[SentToWorkerNodeUri] ,[Result] 
											FROM [Stardust].[Job] WITH(NOLOCK) WHERE JobId = @jobId";
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var selectCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					selectCommand.Parameters.AddWithValue("@JobId", jobId);
					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return job;
						sqlDataReader.Read();
						job = createJobFromSqlDataReader(sqlDataReader);
						setTotalDuration(job);
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
				const string selectCommandText = @"SELECT Id, JobId, Created, Detail FROM [Stardust].[JobDetail] WITH (NOLOCK) WHERE JobId = @JobId ORDER BY Created desc";
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var selectCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					selectCommand.Parameters.AddWithValue("@JobId", jobId);
					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return jobDetails;
						while (sqlDataReader.Read())
						{
							var jobDetail = createJobDetailFromSqlDataReader(sqlDataReader);
							jobDetails.Add(jobDetail);
						}
					}
				}
				return jobDetails;
			}
		}

		public WorkerNode WorkerNode(Guid nodeId)
		{
			const string selectCommandText = @"SELECT DISTINCT Id, Url, Heartbeat, Alive, Running FROM (SELECT Id, Url, Heartbeat, Alive, CASE WHEN Url IN 
													(SELECT SentToWorkerNodeUri FROM Stardust.Job WITH(NOLOCK) WHERE Ended IS NULL) THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS Running 
												FROM [Stardust].WorkerNode WITH(NOLOCK)) w WHERE w.Id = @Id";
			var node = new WorkerNode();
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);
				using (var selectCommand = new SqlCommand(selectCommandText, connection))
				{
					selectCommand.Parameters.AddWithValue("@Id", nodeId);
					var reader = selectCommand.ExecuteReader();
					if (!reader.HasRows) return node;
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
			}
			return node;
		}


		public List<WorkerNode> GetAllWorkerNodes()
		{
			var listToReturn = new List<WorkerNode>();

			const string commandText = @"SELECT DISTINCT Id, Url, Heartbeat, Alive, Running FROM (SELECT Id, Url, Heartbeat, Alive, CASE WHEN Url IN 
								(SELECT SentToWorkerNodeUri FROM Stardust.Job WITH(NOLOCK) WHERE Ended IS NULL) THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS Running 
									FROM [Stardust].WorkerNode WITH(NOLOCK)) w";
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.OpenWithRetry(_retryPolicy);
				using (var selectCommand = new SqlCommand(commandText, connection))
				{
					using (var reader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!reader.HasRows) return listToReturn;

						var ordinalPositionForIdField = reader.GetOrdinal("Id");
						var ordinalPositionForUrlField = reader.GetOrdinal("Url");
						var ordinalPositionForAliveField = reader.GetOrdinal("Alive");
						var ordinalPositionForRunningField = reader.GetOrdinal("Running");
						var ordinalPositionForHeartbeatField = reader.GetOrdinal("Heartbeat");

						while (reader.Read())
						{
							var workerNode = new WorkerNode
							{
								Id = (Guid)reader.GetValue(ordinalPositionForIdField),
								Url = new Uri((string)reader.GetValue(ordinalPositionForUrlField)),
								Alive = (bool)reader.GetValue(ordinalPositionForAliveField),
								Heartbeat = (DateTime)reader.GetValue(ordinalPositionForHeartbeatField),
								Running = (bool)reader.GetValue(ordinalPositionForRunningField),
							};
							listToReturn.Add(workerNode);
						}
					}
				}
			}
			return listToReturn;
		}

		public List<Job> GetAllQueuedJobs(int from, int to)
		{
			var jobs = new List<Job>();

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				const string selectCommandText = @"WITH Ass AS (SELECT top (1000000) *, ROW_NUMBER() OVER (ORDER BY Created asc) AS 'RowNumber'
											FROM Stardust.JobQueue WITH(NOLOCK) ORDER BY Created asc ) SELECT * FROM Ass WITH(NOLOCK) WHERE RowNumber BETWEEN @from AND @to";
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getAllQueuedJobsCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					getAllQueuedJobsCommand.Parameters.AddWithValue("@from", from);
					getAllQueuedJobsCommand.Parameters.AddWithValue("@to", to);
					using (var sqlDataReader = getAllQueuedJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return jobs;
						while (sqlDataReader.Read())
						{
							var job = createQueuedJobFromSqlDataReader(sqlDataReader);
							jobs.Add(job);
						}
					}
				}
			}
			return jobs;
		}

		public Job GetQueuedJob(Guid jobId)
		{
			var job = new Job(); ;
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var selectCommandText = $"SELECT * FROM Stardust.JobQueue WITH(NOLOCK) WHERE JobId = @jobId";
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getQueuedJob = new SqlCommand(selectCommandText, sqlConnection))
				{
					getQueuedJob.Parameters.AddWithValue("@jobId", jobId);
					using (var sqlDataReader = getQueuedJob.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return job;
						sqlDataReader.Read();
						job = createQueuedJobFromSqlDataReader(sqlDataReader);
					}
				}
			}
			return job;
		}

		public void DeleteQueuedJobs(Guid[] jobIds)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				foreach (var ids in jobIds.Batch(400))
				{
					var stringIds = string.Join("','", ids);
					var sql = $@"DELETE FROM Stardust.JobQueue WHERE JobId IN ('{stringIds}')";
					using (var comm = new SqlCommand(sql, sqlConnection))
					{
						comm.ExecuteNonQuery();
					}
				}
			}
		}

		public List<Guid> SelectAllTenants()
		{
			const string selectCommandText = "SELECT * FROM dbo.BusinessUnit WITH(NOLOCK)";
			var ids = new List<Guid>();
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getQueuedJob = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var sqlDataReader = getQueuedJob.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return ids;
						while (sqlDataReader.Read())
						{
							ids.Add(sqlDataReader.GetGuid(0));
						}
					}
				}
			}
			return ids;
		}


		public List<Guid> SelectAllBus(string connString)
		{
			const string selectCommandText = "SELECT Id FROM dbo.BusinessUnit WITH(NOLOCK)";
			var ids = new List<Guid>();
			using (var sqlConnection = new SqlConnection(connString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getQueuedJob = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var sqlDataReader = getQueuedJob.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return ids;
						while (sqlDataReader.Read())
						{
							ids.Add(sqlDataReader.GetGuid(0));
						}
					}
				}
			}
			return ids;
		}

		public object GetAllFailedJobs(int from, int to)
		{
			var jobs = new List<Job>();

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var selectCommandText = $@"WITH Ass AS (SELECT top (1000000) *,  ROW_NUMBER() OVER (ORDER BY Created desc) AS 'RowNumber' 
											FROM Stardust.Job WITH(NOLOCK) WHERE Result LIKE '%Failed%' ORDER BY Created desc ) SELECT * FROM Ass WITH(NOLOCK) WHERE RowNumber BETWEEN @from AND @to";
				sqlConnection.OpenWithRetry(_retryPolicy);
				using (var getAllJobsCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					getAllJobsCommand.Parameters.AddWithValue("@from", from);
					getAllJobsCommand.Parameters.AddWithValue("@to", to);
					using (var sqlDataReader = getAllJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (!sqlDataReader.HasRows) return jobs;
						while (sqlDataReader.Read())
						{
							var job = createJobFromSqlDataReader(sqlDataReader);
							setTotalDuration(job);
							jobs.Add(job);
						}
					}
				}
			}
			return jobs;
		}

		private static T getDBNullSafeValue<T>(object value) where T : class
		{
			return value == DBNull.Value ? null : (T)value;
		}


		private static Job createJobFromSqlDataReader(SqlDataReader sqlDataReader)
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

		private static Job createQueuedJobFromSqlDataReader(SqlDataReader sqlDataReader)
		{
			var job = new Job
			{
				JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
				Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name")),
				CreatedBy = sqlDataReader.GetString(sqlDataReader.GetOrdinal("CreatedBy")),
				Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created")),
				Serialized = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Serialized"))
			};
			return job;
		}

		private static JobDetail createJobDetailFromSqlDataReader(SqlDataReader sqlDataReader)
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
			return reader.IsDBNull(ordinalPosition) ? null : reader.GetString(ordinalPosition);
		}
	}
}
