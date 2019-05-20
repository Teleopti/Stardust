using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net;
using Polly.Retry;
using Stardust.Manager.Extensions;
using Stardust.Manager.Models;
using Stardust.Manager.Policies;

namespace Stardust.Manager.Helpers
{
	public class JobRepositoryCommandExecuter
	{
		private readonly HalfNodesAffinityPolicy _halfNodesAffinityPolicy;
		private readonly RetryPolicy _retryPolicy;
		private static readonly ILog ManagerLogger = LogManager.GetLogger("Stardust.ManagerLog");

		public JobRepositoryCommandExecuter(RetryPolicyProvider retryPolicyProvider, HalfNodesAffinityPolicy halfNodesAffinityPolicy)
		{
			_halfNodesAffinityPolicy = halfNodesAffinityPolicy;
			_retryPolicy = retryPolicyProvider.GetPolicy();
		}

		public void DeleteJobFromJobQueue(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string deleteItemFromJobQueueItemCommandText = @"DELETE FROM [Stardust].[JobQueue] WHERE JobId = @JobId";
			using (var deleteFromJobQueueCommand = new SqlCommand(deleteItemFromJobQueueItemCommandText, sqlConnection, sqlTransaction))
			{
				deleteFromJobQueueCommand.Parameters.AddWithValue("@JobId", jobId);
                _retryPolicy.Execute(deleteFromJobQueueCommand.ExecuteNonQuery);
			}
		}

		public void InsertJobDetail(Guid jobId, string detail, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string insertIntoJobDetailCommandText = @"INSERT INTO [Stardust].[JobDetail]
																([JobId]
																,[Created]
																,[Detail])
															VALUES
																(@JobId
																,@created
																,@Detail)";

			using (var insertJobDetailCommand = new SqlCommand(insertIntoJobDetailCommandText, sqlConnection, sqlTransaction))
			{
				insertJobDetailCommand.Parameters.AddWithValue("@JobId", jobId);
				insertJobDetailCommand.Parameters.AddWithValue("@Detail", detail);
				insertJobDetailCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);

                _retryPolicy.Execute(insertJobDetailCommand.ExecuteNonQuery);
			}
		}

		public void InsertIntoJobQueue(JobQueueItem jobQueueItem, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string insertIntoJobQueueCommandText = @"INSERT INTO [Stardust].[JobQueue]
							   ([JobId],
								[Name],
								[Type],
								[Serialized],
								[CreatedBy],
								[Created],
								[Policy])
							VALUES 
								(@JobId,
								 @Name,
								 @Type,
								 @Serialized,
								 @CreatedBy,
								 @Created,
								 @Policy)";

			using (var insertIntojobQueueCommand = new SqlCommand(insertIntoJobQueueCommandText, sqlConnection, sqlTransaction))
			{
				insertIntojobQueueCommand.Parameters.AddWithValue("@JobId", jobQueueItem.JobId);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Name", jobQueueItem.Name);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Type", jobQueueItem.Type);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Serialized", jobQueueItem.Serialized);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
				insertIntojobQueueCommand.Parameters.AddWithValue("@CreatedBy", jobQueueItem.CreatedBy);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Policy", jobQueueItem.Policy);

                _retryPolicy.Execute(insertIntojobQueueCommand.ExecuteNonQuery);
			}
		}

		public void InsertIntoJob(JobQueueItem jobQueueItem, string sentToWorkerNodeUri, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string insertIntoJobCommandText = @"INSERT INTO [Stardust].[Job]
								   ([JobId]
								   ,[Name]
								   ,[Created]
								   ,[CreatedBy]
								   ,[Started]
								   ,[Ended]
								   ,[Serialized]
								   ,[Type]
								   ,[SentToWorkerNodeUri]
								   ,[Result]
								   ,[Policy])
							 VALUES
								   (@JobId
								   ,@Name
								   ,@Created
								   ,@CreatedBy
								   ,@Started
								   ,@Ended
								   ,@Serialized
								   ,@Type
								   ,@SentToWorkerNodeUri
								   ,@Result
								   ,@Policy)";

			using (var insertIntoJobCommand = new SqlCommand(insertIntoJobCommandText, sqlConnection, sqlTransaction))
			{
				insertIntoJobCommand.Parameters.AddWithValue("@Result", DBNull.Value);
				insertIntoJobCommand.Parameters.AddWithValue("@JobId", jobQueueItem.JobId);
				insertIntoJobCommand.Parameters.AddWithValue("@Name", jobQueueItem.Name);
				insertIntoJobCommand.Parameters.AddWithValue("@Type", jobQueueItem.Type);
				insertIntoJobCommand.Parameters.AddWithValue("@Serialized", jobQueueItem.Serialized);
				insertIntoJobCommand.Parameters.AddWithValue("@Started", DateTime.UtcNow);
				insertIntoJobCommand.Parameters.AddWithValue("@SentToWorkerNodeUri", sentToWorkerNodeUri);
				insertIntoJobCommand.Parameters.AddWithValue("@CreatedBy", jobQueueItem.CreatedBy);
				insertIntoJobCommand.Parameters.AddWithValue("@Created", jobQueueItem.Created);
				insertIntoJobCommand.Parameters.AddWithValue("@Ended", DBNull.Value);
				insertIntoJobCommand.Parameters.AddWithValue("@Policy", jobQueueItem.Policy ?? (object) DBNull.Value);
                _retryPolicy.Execute(insertIntoJobCommand.ExecuteNonQuery);
			}
		}

		public List<JobQueueItem> SelectValidItemsInJobQueue(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string selectAllItemsInJobQueueCommandText = @"SELECT 
										[JobId],
										[Name],
										[Type],
										[Serialized],
										[CreatedBy],
										[Created],
										[Policy]
									FROM [Stardust].[JobQueue] where locktimestamp is null or datediff(minute,GETDATE(),lockTimestamp) < 0 ORDER BY Created";
			return JobQueueItems(sqlConnection, sqlTransaction, selectAllItemsInJobQueueCommandText);
		}

		public List<JobQueueItem> SelectAllItemsInJobQueue(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string selectAllItemsInJobQueueCommandText = @"SELECT 
										[JobId],
										[Name],
										[Type],
										[Serialized],
										[CreatedBy],
										[Created],
										[Policy]
									FROM [Stardust].[JobQueue]";
			return JobQueueItems(sqlConnection, sqlTransaction, selectAllItemsInJobQueueCommandText);
		}

		private List<JobQueueItem> JobQueueItems(SqlConnection sqlConnection, SqlTransaction sqlTransaction,
			string selectAllItemsInJobQueueCommandText)
		{
			var listToReturn = new List<JobQueueItem>();
			using (var sqlCommand = new SqlCommand(selectAllItemsInJobQueueCommandText, sqlConnection, sqlTransaction))
			{
				using (var sqlDataReader = _retryPolicy.Execute(sqlCommand.ExecuteReader))
				{
					if (!sqlDataReader.HasRows) return listToReturn;
					while (sqlDataReader.Read())
					{
						var jobQueueItem = CreateJobQueueItemFromSqlDataReader(sqlDataReader);
						listToReturn.Add(jobQueueItem);
					}
				}
			}
			return listToReturn;
		}

		public List<Uri> SelectAllAvailableWorkerNodes(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			var allAliveWorkerNodesUri = new List<Uri> ();

			const string selectAllAliveWorkerNodesCommandText = @"SELECT DISTINCT Id, Url, Heartbeat, Alive, Running FROM (SELECT Id, Url, Heartbeat, Alive, CASE WHEN Url IN 
								(SELECT SentToWorkerNodeUri FROM Stardust.Job WITH(NOLOCK) WHERE Ended IS NULL) THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS Running 
									FROM [Stardust].WorkerNode WITH(NOLOCK)) w WHERE w.Alive = 1 AND w.Running = 0 ";

			using (var selectAllAliveWorkerNodesCommand = new SqlCommand(selectAllAliveWorkerNodesCommandText, sqlConnection, sqlTransaction))
			{
				using (var readerAliveWorkerNodes = _retryPolicy.Execute(()=>selectAllAliveWorkerNodesCommand.ExecuteReader()))
				{
					if (!readerAliveWorkerNodes.HasRows) return allAliveWorkerNodesUri;

					var ordinalPosForUrl = readerAliveWorkerNodes.GetOrdinal("Url");
					while (readerAliveWorkerNodes.Read())
					{
						allAliveWorkerNodesUri.Add(new Uri(readerAliveWorkerNodes.GetString(ordinalPosForUrl)));
					}
				}
			}
			return allAliveWorkerNodesUri;
		}

		public void UpdateResult(Guid jobId, string result, DateTime ended,SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string updateJobCommandText = @"UPDATE [Stardust].[Job] 
									   SET Result = @Result, Ended = @Ended
										WHERE JobId = @JobId";

			using (var updateResultCommand = new SqlCommand(updateJobCommandText, sqlConnection, sqlTransaction))
			{
				updateResultCommand.Parameters.AddWithValue("@JobId", jobId);
				updateResultCommand.Parameters.AddWithValue("@Result", result);
				updateResultCommand.Parameters.AddWithValue("@Ended", ended);

                _retryPolicy.Execute(updateResultCommand.ExecuteNonQuery);
			}
		}

		public JobQueueItem SelectJobQueueItem(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			JobQueueItem jobQueueItem;
			const string selectJobQueueItemCommandText =
							@"SELECT  [JobId]
								  ,[Name]
								  ,[Serialized]
								  ,[Type]
								  ,[CreatedBy]
								  ,[Created]
								  ,[Policy]
						  FROM [Stardust].[JobQueue]
						  WHERE JobId = @JobId";
			using (var sqlSelectCommand = new SqlCommand(selectJobQueueItemCommandText, sqlConnection, sqlTransaction))
			{
				sqlSelectCommand.Parameters.AddWithValue("@JobId", jobId);
				using (var sqlDataReader = _retryPolicy.Execute(sqlSelectCommand.ExecuteReader))
				{
					if (!sqlDataReader.HasRows) return null;
					sqlDataReader.Read();
					jobQueueItem = CreateJobQueueItemFromSqlDataReader(sqlDataReader);
				}
			}
			return jobQueueItem;
		}

		public Job SelectJob(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			Job job;
			const string selectJobByJobIdCommandText = @"SELECT [JobId]
														,[Name]
														,[Created]
														,[CreatedBy]
														,[Started]
														,[Ended]
														,[Serialized]
														,[Type]
														,[SentToWorkerNodeUri]
														,[Result]
														,[Policy]
													FROM [Stardust].[Job]
													WHERE  JobId = @JobId";

			using (var selectJobByJobIdCommand = new SqlCommand(selectJobByJobIdCommandText, sqlConnection, sqlTransaction))
			{
				selectJobByJobIdCommand.Parameters.AddWithValue("@JobId", jobId);
				using (var sqlDataReader = _retryPolicy.Execute(selectJobByJobIdCommand.ExecuteReader))
				{
					if (!sqlDataReader.HasRows) return null;
					sqlDataReader.Read();
					job = CreateJobFromSqlDataReader(sqlDataReader);
				}
			}
			return job;
		}

		public List<Job> SelectAllJobs(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			var jobs = new List<Job>();
			const string selectCommandText = @"SELECT  [JobId]
											  ,[Name]
											  ,[Created]
											  ,[CreatedBy]
											  ,[Started]
											  ,[Ended]
											  ,[Serialized]
											  ,[Type]
											  ,[SentToWorkerNodeUri]
											  ,[Result]
											  ,[Policy]
										  FROM [Stardust].[Job] WITH (NOLOCK)";

			using (var getAllJobsCommand = new SqlCommand(selectCommandText, sqlConnection, sqlTransaction))
			{
				using (var sqlDataReader = _retryPolicy.Execute(getAllJobsCommand.ExecuteReader))
				{
					if (!sqlDataReader.HasRows) return jobs;
					while (sqlDataReader.Read())
					{
						var job = CreateJobFromSqlDataReader(sqlDataReader);
						jobs.Add(job);
					}
				}
			}
			return jobs;
		}

		public List<Job> SelectAllExecutingJobs(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			var jobs = new List<Job>();
			const string selectCommandText = @"SELECT  [JobId]
											  ,[Name]
											  ,[CreatedBy]
											  ,[Created]
											  ,[Started]
											  ,[Ended]
											  ,[Serialized]
											  ,[Type]
											  ,[SentToWorkerNodeUri]
											  ,[Result]
											  ,[Policy]
										  FROM [Stardust].[Job] WITH (NOLOCK)
										  WHERE [Started] IS NOT NULL AND [Ended] IS NULL";

			using (var getAllExecutingJobsCommand = new SqlCommand(selectCommandText, sqlConnection, sqlTransaction))
			{
				using (var sqlDataReader = _retryPolicy.Execute(getAllExecutingJobsCommand.ExecuteReader))
				{
					if (!sqlDataReader.HasRows) return jobs;
					while (sqlDataReader.Read())
					{
						var job = CreateJobFromSqlDataReader(sqlDataReader);
						jobs.Add(job);
					}
				}
			}
			return jobs;
		}

		public List<JobDetail> SelectJobDetails(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			var jobDetails = new List<JobDetail>();
			const string selectCommandText = @"SELECT  
											Id, 
											JobId, 
											Created, 
											Detail  
										FROM [Stardust].[JobDetail] WITH (NOLOCK) 
										WHERE JobId = @JobId";

			using (var selectJobDetailByJobIdCommand = new SqlCommand(selectCommandText, sqlConnection, sqlTransaction))
			{
				selectJobDetailByJobIdCommand.Parameters.AddWithValue("@JobId", jobId);
				using (var sqlDataReader = _retryPolicy.Execute(selectJobDetailByJobIdCommand.ExecuteReader))
				{
					if (!sqlDataReader.HasRows) return jobDetails;
					while (sqlDataReader.Read())
					{
						var jobDetail = CreateJobDetailFromSqlDataReader(sqlDataReader);
						jobDetails.Add(jobDetail);
					}
				}
			}
			return jobDetails;
		}

		public Job SelectExecutingJob(string workerNodeUri, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			Job job;
			const string selectJobThatDidNotEndCommandText = @"SELECT [JobId]
											  ,[Name]
											  ,[Created]
											  ,[CreatedBy]
											  ,[Started]
											  ,[Ended]
											  ,[Serialized]
											  ,[Type]
											  ,[SentToWorkerNodeUri]
											  ,[Result]
											  ,[Policy]
										  FROM [Stardust].[Job]
										  WHERE  SentToWorkerNodeUri = @SentToWorkerNodeUri AND
												 Started IS NOT NULL AND 
												 Ended IS NULL";
			
			using (var selectJobThatDidNotEndCommand = new SqlCommand(selectJobThatDidNotEndCommandText, sqlConnection, sqlTransaction))
			{
				selectJobThatDidNotEndCommand.Parameters.AddWithValue("@SentToWorkerNodeUri", workerNodeUri);
				using (var sqlDataReader = _retryPolicy.Execute(selectJobThatDidNotEndCommand.ExecuteReader))
				{
					if (!sqlDataReader.HasRows) return null;
					sqlDataReader.Read();
					job = CreateJobFromSqlDataReader(sqlDataReader);
				}
			}
			return job;
		}


		public void DeleteJob(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string deleteJobByIdSqlCommandText = @"DELETE FROM [Stardust].[Job] WHERE JobId = @JobId";
			using (var deleteJobByJobIdCommand = new SqlCommand(deleteJobByIdSqlCommandText, sqlConnection, sqlTransaction))
			{
				deleteJobByJobIdCommand.Parameters.AddWithValue("@JobId", jobId);
                _retryPolicy.Execute(deleteJobByJobIdCommand.ExecuteNonQuery);
			}
		}


		public string SelectWorkerNode(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			string sentToWorkerNodeUri;
			const string selectWorkerNodeUriFromJobCommandText = "SELECT SentToWorkerNodeUri FROM [Stardust].[Job] WHERE JobId = @JobId";
			using (var createSelectWorkerNodeUriCommand = new SqlCommand(selectWorkerNodeUriFromJobCommandText, sqlConnection, sqlTransaction))
			{
				createSelectWorkerNodeUriCommand.Parameters.AddWithValue("@JobId", jobId);
                using (var selectSqlReader = _retryPolicy.Execute(createSelectWorkerNodeUriCommand.ExecuteReader))
                {
                    if (!selectSqlReader.HasRows) return null;
                    selectSqlReader.Read();
                    sentToWorkerNodeUri = selectSqlReader.GetString(selectSqlReader.GetOrdinal("SentToWorkerNodeUri"));
                }
			}
			return sentToWorkerNodeUri;
		}

		public int GetNumberOfAliveNodes(SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			const string selectCommand = @"SELECT COUNT(*) 
											FROM [Stardust].[WorkerNode]
											WHERE Alive = 1";
			int count;
			using (var sqlCommand = new SqlCommand(selectCommand, sqlConnection, sqlTransaction))
			{
				count = Convert.ToInt32(_retryPolicy.Execute(sqlCommand.ExecuteScalar));
			}
			return count;
		}

		public JobQueueItem AcquireJobQueueItem(SqlConnection sqlConnection)
		{
			JobQueueItem jobQueueItem;

			using (var sqlTransaction = sqlConnection.BeginTransaction())
			{
				var validJobsInQueue = SelectValidItemsInJobQueue(sqlConnection, sqlTransaction);
				var allExecutingJobs = SelectAllExecutingJobs(sqlConnection, sqlTransaction);
				var numberOfAliveNodes = GetNumberOfAliveNodes(sqlConnection, sqlTransaction);
				Guid? jobId = null;
				foreach (var queueItem in validJobsInQueue)
				{
					if (_halfNodesAffinityPolicy.CheckPolicy(queueItem, allExecutingJobs, numberOfAliveNodes))
					{
						jobId = queueItem.JobId;
						break;
					}
				}
				if (!jobId.HasValue)
					return null;

				using (var selectJobQueueItemCommand = new SqlCommand("[Stardust].[AcquireQueuedJob]", sqlConnection, sqlTransaction))
				{
					ManagerLogger.Info($"Move job {jobId} from queue to running phase.");
					selectJobQueueItemCommand.CommandType = CommandType.StoredProcedure;

					var parameter = new SqlParameter("@jobId", SqlDbType.UniqueIdentifier) { Value = jobId.Value };
					selectJobQueueItemCommand.Parameters.Add(parameter);

					using (var reader = _retryPolicy.Execute(selectJobQueueItemCommand.ExecuteReader))
					{
						if (!reader.HasRows) return null;
						reader.Read();

						jobQueueItem = CreateJobQueueItemFromSqlDataReader(reader);
					}
				}
				sqlTransaction.Commit();

			}
			return jobQueueItem;
		}

		public void TagQueueItem(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string commandText = "update [Stardust].[JobQueue] set lockTimestamp = NULL where JobId = @Id";

			using (var cmd = new SqlCommand(commandText, sqlConnection, sqlTransaction))
			{
				cmd.Parameters.AddWithValue("@Id", jobId);
				cmd.ExecuteNonQuery();
			}
		}

		public int SelectCountJobQueueItem(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			int count;
			const string selectCommand = @"SELECT COUNT(*) 
											FROM [Stardust].[JobQueue]
											WHERE JobId = @JobId";
			using (var sqlCommand = new SqlCommand(selectCommand, sqlConnection, sqlTransaction))
			{
				sqlCommand.Parameters.AddWithValue("@JobId", jobId);
				count = Convert.ToInt32(sqlCommand.ExecuteScalar());
			}
			return count;
		}

		private static JobQueueItem CreateJobQueueItemFromSqlDataReader(SqlDataReader sqlDataReader)
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
				Name = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Name")),
				Serialized = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Serialized")).Replace(@"\", @""),
				Type = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Type")),
				CreatedBy = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("CreatedBy")),
				Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created")),
				Policy = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Policy"))
			};
			return jobQueueItem;
		}

		private static Job CreateJobFromSqlDataReader(SqlDataReader sqlDataReader)
		{
			var job = new Job
			{
				JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
				Name = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Name")),
				SentToWorkerNodeUri = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("SentToWorkerNodeUri")),
				Type = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Type")),
				CreatedBy = sqlDataReader.GetString(sqlDataReader.GetOrdinal("CreatedBy")),
				Result = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Result")),
				Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created")),
				Serialized = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Serialized")),
				Started = sqlDataReader.GetNullableDateTime(sqlDataReader.GetOrdinal("Started")),
				Ended = sqlDataReader.GetNullableDateTime(sqlDataReader.GetOrdinal("Ended")),
				Policy = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Policy"))
			};
			return job;
		}

		private static JobDetail CreateJobDetailFromSqlDataReader(SqlDataReader sqlDataReader)
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

	
}
