﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Stardust.Manager.Extensions;
using Stardust.Manager.Models;

namespace Stardust.Manager.Helpers
{
	public class JobRepositoryCommandExecuter
	{
		private readonly RetryPolicy _retryPolicy;

		public JobRepositoryCommandExecuter(RetryPolicyProvider retryPolicyProvider)
		{
			_retryPolicy = retryPolicyProvider.GetPolicy();
		}

		public void DeleteJobFromJobQueue(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string deleteItemFromJobQueueItemCommandText = @"DELETE FROM [Stardust].[JobQueue] WHERE JobId = @JobId";
			using (var deleteFromJobQueueCommand = new SqlCommand(deleteItemFromJobQueueItemCommandText, sqlConnection, sqlTransaction))
			{
				deleteFromJobQueueCommand.Parameters.AddWithValue("@JobId", jobId);
				deleteFromJobQueueCommand.ExecuteNonQueryWithRetry(_retryPolicy);
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

				insertJobDetailCommand.ExecuteNonQueryWithRetry(_retryPolicy);
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
								[Created])
							VALUES 
								(@JobId,
								 @Name,
								 @Type,
								 @Serialized,
								 @CreatedBy,
								 @Created)";

			using (var insertIntojobQueueCommand = new SqlCommand(insertIntoJobQueueCommandText, sqlConnection, sqlTransaction))
			{
				insertIntojobQueueCommand.Parameters.AddWithValue("@JobId", jobQueueItem.JobId);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Name", jobQueueItem.Name);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Type", jobQueueItem.Type);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Serialized", jobQueueItem.Serialized);
				insertIntojobQueueCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
				insertIntojobQueueCommand.Parameters.AddWithValue("@CreatedBy", jobQueueItem.CreatedBy);

				insertIntojobQueueCommand.ExecuteNonQueryWithRetry(_retryPolicy);
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
								   ,[Result])
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
								   ,@Result)";

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
				insertIntoJobCommand.ExecuteNonQueryWithRetry(_retryPolicy);
			}
		}

		public List<JobQueueItem> SelectAllItemsInJobQueue(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			var listToReturn = new List<JobQueueItem>();
			const string selectAllItemsInJobQueueCommandText = @"SELECT 
										[JobId],
										[Name],
										[Type],
										[Serialized],
										[CreatedBy],
										[Created]
									FROM [Stardust].[JobQueue]";
			using (var sqlCommand = new SqlCommand(selectAllItemsInJobQueueCommandText, sqlConnection, sqlTransaction))
			{
				using (var sqlDataReader = sqlCommand.ExecuteReaderWithRetry(_retryPolicy))
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

		public List<Uri> SelectAllAliveWorkerNodes(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			var allAliveWorkerNodesUri = new List<Uri> ();
			const string selectAllAliveWorkerNodesCommandText = @"SELECT DISTINCT Id, Url, Heartbeat, Alive, Running FROM (SELECT Id, Url, Heartbeat, Alive, CASE WHEN Url IN 
								(SELECT SentToWorkerNodeUri FROM Stardust.Job WITH(NOLOCK) WHERE Ended IS NULL) THEN CONVERT(bit,1) ELSE CONVERT(bit,0) END AS Running 
									FROM [Stardust].WorkerNode WITH(NOLOCK)) w order by Running";
			using (var selectAllAliveWorkerNodesCommand = new SqlCommand(selectAllAliveWorkerNodesCommandText, sqlConnection, sqlTransaction))
			{
				using (var readerAliveWorkerNodes = selectAllAliveWorkerNodesCommand.ExecuteReaderWithRetry(_retryPolicy))
				{
					if (readerAliveWorkerNodes.HasRows)
					{
						var ordinalPosForUrl = readerAliveWorkerNodes.GetOrdinal("Url");
						while (readerAliveWorkerNodes.Read())
						{
							allAliveWorkerNodesUri.Add(new Uri(readerAliveWorkerNodes.GetString(ordinalPosForUrl)));
						}
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

				updateResultCommand.ExecuteNonQueryWithRetry(_retryPolicy);
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
						  FROM [Stardust].[JobQueue]
						  WHERE JobId = @JobId";
			using (var sqlSelectCommand = new SqlCommand(selectJobQueueItemCommandText, sqlConnection, sqlTransaction))
			{
				sqlSelectCommand.Parameters.AddWithValue("@JobId", jobId);
				using (var sqlDataReader = sqlSelectCommand.ExecuteReaderWithRetry(_retryPolicy))
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
													FROM [Stardust].[Job]
													WHERE  JobId = @JobId";

			using (var selectJobByJobIdCommand = new SqlCommand(selectJobByJobIdCommandText, sqlConnection, sqlTransaction))
			{
				selectJobByJobIdCommand.Parameters.AddWithValue("@JobId", jobId);
				using (var sqlDataReader = selectJobByJobIdCommand.ExecuteReaderWithRetry(_retryPolicy))
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
										  FROM [Stardust].[Job] WITH (NOLOCK)";

			using (var getAllJobsCommand = new SqlCommand(selectCommandText, sqlConnection, sqlTransaction))
			{
				using (var sqlDataReader = getAllJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
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
										  FROM [Stardust].[Job] WITH (NOLOCK)
										  WHERE [Started] IS NOT NULL AND [Ended] IS NULL";

			using (var getAllExecutingJobsCommand = new SqlCommand(selectCommandText, sqlConnection, sqlTransaction))
			{
				using (var sqlDataReader = getAllExecutingJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
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
				using (var sqlDataReader = selectJobDetailByJobIdCommand.ExecuteReaderWithRetry(_retryPolicy))
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
										  FROM [Stardust].[Job]
										  WHERE  SentToWorkerNodeUri = @SentToWorkerNodeUri AND
												 Started IS NOT NULL AND 
												 Ended IS NULL";
			
			using (var selectJobThatDidNotEndCommand = new SqlCommand(selectJobThatDidNotEndCommandText, sqlConnection, sqlTransaction))
			{
				selectJobThatDidNotEndCommand.Parameters.AddWithValue("@SentToWorkerNodeUri", workerNodeUri);
				using (var sqlDataReader = selectJobThatDidNotEndCommand.ExecuteReaderWithRetry(_retryPolicy))
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
				deleteJobByJobIdCommand.ExecuteNonQueryWithRetry(_retryPolicy);
			}
		}


		public string SelectWorkerNode(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			string sentToWorkerNodeUri;
			const string selectWorkerNodeUriFromJobCommandText = "SELECT SentToWorkerNodeUri FROM [Stardust].[Job] WHERE JobId = @JobId";
			using (var createSelectWorkerNodeUriCommand = new SqlCommand(selectWorkerNodeUriFromJobCommandText, sqlConnection, sqlTransaction))
			{
				createSelectWorkerNodeUriCommand.Parameters.AddWithValue("@JobId", jobId);
				using (var selectSqlReader = createSelectWorkerNodeUriCommand.ExecuteReaderWithRetry(_retryPolicy))
				{
					if (!selectSqlReader.HasRows) return null;
					selectSqlReader.Read();
					sentToWorkerNodeUri = selectSqlReader.GetString(selectSqlReader.GetOrdinal("SentToWorkerNodeUri"));
				}
			}
			return sentToWorkerNodeUri;
		}

		public JobQueueItem AcquireJobQueueItem(SqlConnection sqlConnection)
		{
			JobQueueItem jobQueueItem;
			using (var sqlTransaction = sqlConnection.BeginTransaction())
			{
				using (
					var selectJobQueueItemCommand = new SqlCommand("[Stardust].[AcquireQueuedJob]", sqlConnection, sqlTransaction))
				{
					selectJobQueueItemCommand.CommandType = CommandType.StoredProcedure;

					var retVal = new SqlParameter("@idd", SqlDbType.UniqueIdentifier)
						{Direction = ParameterDirection.ReturnValue};
					selectJobQueueItemCommand.Parameters.Add(retVal);

					using (var reader = selectJobQueueItemCommand.ExecuteReaderWithRetry(_retryPolicy))
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

		public void UpdateWorkerNode(bool alive, string availableNode, SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
		{
			const string updateCommandText = @"UPDATE [Stardust].[WorkerNode] 
											SET Alive = @Alive
											WHERE Url = @Url";

			using (var command = new SqlCommand(updateCommandText, sqlConnection, sqlTransaction))
			{
				command.Parameters.Add("@Alive", SqlDbType.Bit).Value = alive;
				command.Parameters.Add("@Url", SqlDbType.NVarChar).Value = availableNode;
				command.ExecuteNonQueryWithRetry(_retryPolicy);
			}
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
				Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created"))
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
				Ended = sqlDataReader.GetNullableDateTime(sqlDataReader.GetOrdinal("Ended"))
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
