using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Stardust;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.Repositories.Stardust
{
	public class StardustRepository : IStardustRepository
	{
		private const int delayMs = 100;

		private readonly string _connectionString;
		private readonly Policy _retryPolicy;

		public StardustRepository(string connectionString)
		{
			_connectionString = connectionString;
			_retryPolicy = Policy.Handle<TimeoutException>()
				.Or<SqlException>(DetectTransientSqlException.IsTransient)
				.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
				.WaitAndRetry(5, i => TimeSpan.FromMilliseconds(delayMs));
		}

		public IList<Job> GetJobsByNodeId(Guid nodeId, int from, int to)
		{
			var jobs = new List<Job>();

			using (var connection = new SqlConnection(_connectionString))
			{
				const string selectCommandText = @"WITH Ass AS (SELECT top (1000000) *, ROW_NUMBER() OVER (ORDER BY Started desc) AS 'RowNumber' FROM (SELECT * FROM [Stardust].Job
								WHERE SentToWorkerNodeUri IN (SELECT Url FROM [Stardust].WorkerNode WHERE Id = @nodeId)) as b ORDER BY Started desc ) 
								SELECT * FROM Ass WITH(NOLOCK) WHERE RowNumber BETWEEN @from AND @to";

				_retryPolicy.Execute(connection.Open);
				using (var selectCommand = new SqlCommand(selectCommandText, connection))
				{
					selectCommand.Parameters.AddWithValue("@nodeId", nodeId);
					selectCommand.Parameters.AddWithValue("@from", from);
					selectCommand.Parameters.AddWithValue("@to", to);
					using (var reader = _retryPolicy.Execute(selectCommand.ExecuteReader))
					{
						if (!reader.HasRows) return jobs;
						while (reader.Read())
						{
							var job = createJobFromSqlDataReader(reader);
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


		public List<string> GetAllTypes()
		{
			const string selectCommandText = "SELECT DISTINCT Type FROM Stardust.Job WITH(NOLOCK)";
			var types = new List<string>();
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				_retryPolicy.Execute(sqlConnection.Open);
				using (var selectTypesCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var sqlDataReader = _retryPolicy.Execute(selectTypesCommand.ExecuteReader))
					{
						if (!sqlDataReader.HasRows) return types;
						while (sqlDataReader.Read())
						{
							var type = sqlDataReader.GetString(0);
							if (type.Contains("."))
								type = type.Substring(type.LastIndexOf('.') + 1);
							types.Add(type);
						}
					}
				}
			}
			return types;
		}

		public List<string> GetAllTypesInQueue()
		{
			const string selectCommandText = "SELECT DISTINCT Type FROM Stardust.JobQueue WITH(NOLOCK)";
			var types = new List<string>();
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				_retryPolicy.Execute(sqlConnection.Open);
				using (var selectTypesCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var sqlDataReader = _retryPolicy.Execute(selectTypesCommand.ExecuteReader))
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

		public Job GetOldestJob()
		{
			Job job = null;
			const string selectCommandText = "SELECT TOP 1 * FROM Stardust.Job order By Created asc";
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				_retryPolicy.Execute(sqlConnection.Open);
				using (var getOldestJobCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var reader = _retryPolicy.Execute(getOldestJobCommand.ExecuteReader))
					{
						if (!reader.HasRows) return job;
						while (reader.Read())
						{
							job = createJobFromSqlDataReader(reader);
						}
					}
				}
			}
			return job;
		}

		public int GetQueueCount()
		{
			const string selectCommandText = "SELECT count(*) FROM Stardust.JobQueue";
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				_retryPolicy.Execute(sqlConnection.Open);
				using (var countCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var reader = _retryPolicy.Execute(countCommand.ExecuteReader))
					{
						if (!reader.HasRows) return 0;
						while (reader.Read())
						{
							return reader.GetInt32(0);
						}
					}
				}
			}
			return 0;
		}

		public IList<Job> GetJobs(JobFilterModel filter)
		{
			var jobs = new List<Job>();

			var selectCommandText = $@"SELECT TOP {filter.To} * FROM Stardust.Job WHERE 1=1 ";

			if (filter.DataSource != null)
				selectCommandText = selectCommandText + $@"AND Serialized LIKE '%LogOnDatasource"":""{filter.DataSource}%' ";

			if (filter.Type != null)
				selectCommandText = selectCommandText + $@"AND Type LIKE '%.{filter.Type}' ";

			if (filter.FromDate != null && filter.ToDate != null)
				selectCommandText = selectCommandText + $@"AND Started BETWEEN '{filter.FromDate.Value.Date:yyyy-MM-dd}' AND '{filter.ToDate.Value.Date.AddDays(1):yyyy-MM-dd}' ";

			if (filter.Result != null)
				selectCommandText = selectCommandText + $@"AND Result = '{filter.Result}'";

			selectCommandText = selectCommandText + "ORDER BY Created DESC";

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				_retryPolicy.Execute(sqlConnection.Open);
				using (var getAllJobsCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var reader = _retryPolicy.Execute(getAllJobsCommand.ExecuteReader))
					{
						if (!reader.HasRows) return jobs;
						while (reader.Read())
						{
							var job = createJobFromSqlDataReader(reader);
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

			var selectCommandText = $@"SELECT TOP {filter.To} * FROM Stardust.JobQueue WHERE 1=1 ";

			if (filter.DataSource != null)
				selectCommandText = selectCommandText + $@"AND Serialized LIKE '%LogOnDatasource"":""{filter.DataSource}%' ";

			if (filter.Type != null)
				selectCommandText = selectCommandText + $@"AND Type = 'Teleopti.Ccc.Domain.ApplicationLayer.Events.{filter.Type}' ";

			selectCommandText = selectCommandText + "ORDER BY Created DESC";

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				_retryPolicy.Execute(sqlConnection.Open);
				using (var getQueuedJobsCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var reader = _retryPolicy.Execute(getQueuedJobsCommand.ExecuteReader))
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
				_retryPolicy.Execute(sqlConnection.Open);
				using (var selectCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					selectCommand.Parameters.AddWithValue("@JobId", jobId);
					using (var sqlDataReader = _retryPolicy.Execute(selectCommand.ExecuteReader))
					{
						if (!sqlDataReader.HasRows) return job;
						sqlDataReader.Read();
						job = createJobFromSqlDataReader(sqlDataReader);
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
				_retryPolicy.Execute(sqlConnection.Open);
				using (var selectCommand = new SqlCommand(selectCommandText, sqlConnection))
				{
					selectCommand.Parameters.AddWithValue("@JobId", jobId);
					using (var sqlDataReader = _retryPolicy.Execute(selectCommand.ExecuteReader))
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
				_retryPolicy.Execute(connection.Open);
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

			const string commandText = @"SELECT DISTINCT Id, Url, Heartbeat, Alive,Running FROM (SELECT Id, Url, Heartbeat, Alive, CASE WHEN Url IN 
								(SELECT SentToWorkerNodeUri FROM Stardust.Job WITH(NOLOCK) WHERE Ended IS NULL) THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS Running 
									FROM [Stardust].WorkerNode WITH(NOLOCK)) w";
			using (var connection = new SqlConnection(_connectionString))
			{
				_retryPolicy.Execute(connection.Open);
				using (var selectCommand = new SqlCommand(commandText, connection))
				{
					using (var reader = _retryPolicy.Execute(selectCommand.ExecuteReader))
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
								Running = (bool)reader.GetValue(ordinalPositionForRunningField)
							};
							listToReturn.Add(workerNode);
						}
					}
				}
			}
			return listToReturn;
		}
	

		public Job GetQueuedJob(Guid jobId)
		{
			var job = new Job(); ;
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var selectCommandText = $"SELECT * FROM Stardust.JobQueue WITH(NOLOCK) WHERE JobId = @jobId";
				_retryPolicy.Execute(sqlConnection.Open);
				using (var getQueuedJob = new SqlCommand(selectCommandText, sqlConnection))
				{
					getQueuedJob.Parameters.AddWithValue("@jobId", jobId);
					using (var sqlDataReader = _retryPolicy.Execute(getQueuedJob.ExecuteReader))
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
				_retryPolicy.Execute(sqlConnection.Open);
				foreach (var ids in jobIds.Batch(400))
				{
					var stringIds = string.Join("','", ids);
					var sql = $@"DELETE FROM Stardust.JobQueue WHERE JobId IN ('{stringIds}')";
					using (var comm = new SqlCommand(sql, sqlConnection))
					{
						_retryPolicy.Execute(comm.ExecuteNonQuery);
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
				_retryPolicy.Execute(sqlConnection.Open);
				using (var getQueuedJob = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var sqlDataReader = _retryPolicy.Execute(getQueuedJob.ExecuteReader))
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
				_retryPolicy.Execute(sqlConnection.Open);
				using (var getQueuedJob = new SqlCommand(selectCommandText, sqlConnection))
				{
					using (var sqlDataReader = _retryPolicy.Execute(getQueuedJob.ExecuteReader))
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
			setTotalDuration(job);
			return job;
		}

		private static Job createQueuedJobFromSqlDataReader(SqlDataReader sqlDataReader)
		{
			var job = new Job
			{
				JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
				Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name")),
				Type = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Type")),
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
