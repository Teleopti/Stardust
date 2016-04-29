using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Stardust.Manager.Extensions;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class JobRepository : IJobRepository
	{
		private readonly string _connectionString;

		private readonly object _lockAddItemToJobQueue = new object();
		private readonly object _lockTryAssignJobToWorkerNode = new object();

		private readonly RetryPolicy _retryPolicy;
		private readonly RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts> _retryPolicyTimeout;

		public JobRepository(string connectionString,
		                     RetryPolicyProvider retryPolicyProvider)
		{
			if (retryPolicyProvider == null)
			{
				throw new ArgumentNullException("retryPolicyProvider");
			}

			if (retryPolicyProvider.GetPolicy() == null)
			{
				throw new ArgumentNullException("retryPolicyProvider.GetPolicy");
			}

			if (retryPolicyProvider.GetPolicyWithTimeout() == null)
			{
				throw new ArgumentNullException("retryPolicyProvider.GetPolicyWithTimeout");
			}

			_connectionString = connectionString;

			_retryPolicy = retryPolicyProvider.GetPolicy();

			_retryPolicyTimeout = retryPolicyProvider.GetPolicyWithTimeout();
		}

		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			try
			{
				Monitor.Enter(_lockAddItemToJobQueue);

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var sqlCommand =CreateCommandHelper.CreateInsertIntoJobQueueCommand(jobQueueItem, sqlConnection, null))
					{
						sqlCommand.ExecuteNonQueryWithRetry(_retryPolicy);
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
			finally
			{
				Monitor.Exit(_lockAddItemToJobQueue);
			}
		}

		public List<JobQueueItem> GetAllItemsInJobQueue()
		{
			var listToReturn = new List<JobQueueItem>();

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var sqlCommand = CreateSelectAllItemsInJobQueueSqlCommand(sqlConnection))
					{
						using (var sqlDataReader = sqlCommand.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (sqlDataReader.HasRows)
							{
								while (sqlDataReader.Read())
								{
									var jobQueueItem =
										CreateJobQueueItemFromSqlDataReader(sqlDataReader);

									listToReturn.Add(jobQueueItem);
								}
							}
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}

			return listToReturn;
		}
		

		private void DeleteJobQueueItemByJobId(Guid jobId)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var deleteFromJobQueueCommand = CreateDeleteFromJobQueueCommand(jobId, sqlConnection, null))
					{
						deleteFromJobQueueCommand.ExecuteNonQueryWithRetry(_retryPolicy);
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		public void AssignJobToWorkerNode(IHttpSender httpSender)
		{
			try
			{
				Monitor.Enter(_lockTryAssignJobToWorkerNode);

				var allAliveWorkerNodesUri = new List<Uri>();

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicyTimeout);

					using (var selectAllAliveWorkerNodesCommand = CreateCommandHelper.CreateSelectAllAliveWorkerNodesCommand(sqlConnection))
					{
						using (var readerAliveWorkerNodes = selectAllAliveWorkerNodesCommand.ExecuteReader())
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
				}

				if (!allAliveWorkerNodesUri.Any()) return;
				
				foreach (var uri in allAliveWorkerNodesUri)
				{
					try
					{
						var builderHelper = new NodeUriBuilderHelper(uri);
						var isIdleUri = builderHelper.GetIsIdleTemplateUri();

						var response = httpSender.GetAsync(isIdleUri).Result;

						if (response.IsSuccessStatusCode)
						{
							AssignJobToWorkerNodeWorker(httpSender, uri);
						}
					}
					catch
					{
						// continue
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
			finally
			{
				Monitor.Exit(_lockTryAssignJobToWorkerNode);
			}
		}

		public void CancelJobByJobId(Guid jobId, IHttpSender httpSender)
		{
			if (DoesJobQueueItemExists(jobId))
			{
				DeleteJobQueueItemByJobId(jobId);
			}
			else
			{
				CancelJobByJobIdWorker(jobId, httpSender);
			}
		}
		
		public void UpdateResultForJob(Guid jobId,string result, DateTime ended)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);

				using (var updateResultCommand = CreateCommandHelper.CreateUpdateResultCommand(jobId, result, ended, sqlConnection))
				{
					updateResultCommand.ExecuteNonQueryWithRetry(_retryPolicy);
				}
			}
		}

		public void CreateJobDetailByJobId(Guid jobId,string detail, DateTime created)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);

				using (var insertCommand = CreateCommandHelper.CreateInsertIntoJobDetailCommand(jobId, detail, created, sqlConnection))
				{
					insertCommand.ExecuteNonQueryWithRetry(_retryPolicy);
				}
			}
		}
		

		public JobQueueItem GetJobQueueItemByJobId(Guid jobId)
		{
			JobQueueItem jobQueueItem = null;

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var sqlSelectCommand = CreateCommandHelper.CreateGetJobQueueItemByJobIdCommand(jobId, sqlConnection))
					{
						using (var sqlDataReader = sqlSelectCommand.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (sqlDataReader.HasRows)
							{
								sqlDataReader.Read();
								jobQueueItem = CreateJobQueueItemFromSqlDataReader(sqlDataReader);
							}
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}

			return jobQueueItem;
		}

		public Job GetJobByJobId(Guid jobId)
		{
			Job job = null;
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var selectJobByJobIdCommand = CreateCommandHelper.CreateSelectJobByJobIdCommand(jobId, sqlConnection))
					{
						using (var sqlDataReader = selectJobByJobIdCommand.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (sqlDataReader.HasRows)
							{
								sqlDataReader.Read();
								job = CreateJobFromSqlDataReader(sqlDataReader);
							}
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}

			return job;
		}
		

		public IList<Job> GetAllJobs()
		{
			var jobs = new List<Job>();

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var getAllJobsCommand = CreateCommandHelper.CreateGetAllJobsCommand(sqlConnection))
					{
						using (var sqlDataReader = getAllJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (sqlDataReader.HasRows)
							{
								while (sqlDataReader.Read())
								{
									var job = CreateJobFromSqlDataReader(sqlDataReader);
									jobs.Add(job);
								}
							}
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}

			return jobs;
		}


		public IList<Job> GetAllExecutingJobs()
		{
			var jobs = new List<Job>();

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var getAllExecutingJobsCommand = CreateCommandHelper.CreateGetAllExecutingJobsCommand(sqlConnection))
					{
						using (var sqlDataReader = getAllExecutingJobsCommand.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (sqlDataReader.HasRows)
							{
								while (sqlDataReader.Read())
								{
									var job = CreateJobFromSqlDataReader(sqlDataReader);
									jobs.Add(job);
								}
							}
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}

			return jobs;
		}
		

		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			try
			{
				var jobDetails = new List<JobDetail>();

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var selectJobDetailByJobIdCommand = CreateCommandHelper.CreateSelectJobDetailByJobIdCommand(jobId, sqlConnection))
					{
						using (var sqlDataReader = selectJobDetailByJobIdCommand.ExecuteReaderWithRetry(_retryPolicy))
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
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		public void RequeueJobThatDidNotEndByWorkerNodeUri(string workerNodeUri)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						Job job = null;
						using (var selectJobThatDidNotEndCommand = CreateCommandHelper.CreateSelectJobThatDidNotEndCommand(workerNodeUri, sqlConnection, sqlTransaction))
						{
							using (var sqlDataReader = selectJobThatDidNotEndCommand.ExecuteReaderWithRetry(_retryPolicy))
							{
								if (sqlDataReader.HasRows)
								{
									sqlDataReader.Read();
									job = CreateJobFromSqlDataReader(sqlDataReader);
								}
							}
						}
						if (job != null)
						{
							var jobQueueItem = new JobQueueItem
							{
								Created = job.Created,
								CreatedBy = job.CreatedBy,
								JobId = job.JobId,
								Serialized = job.Serialized,
								Name = job.Name,
								Type = job.Type
							};

							using (var insertIntojobQueueCommand = CreateCommandHelper.CreateInsertIntoJobQueueCommand(jobQueueItem, sqlConnection, sqlTransaction))
							{
								insertIntojobQueueCommand.ExecuteNonQueryWithRetry(_retryPolicy);
							}
							using (var deleteJobByJobIdCommand = CreateCommandHelper.CreateDeleteJobByJobIdCommand(jobQueueItem.JobId, sqlConnection, sqlTransaction))
							{
								deleteJobByJobIdCommand.ExecuteNonQueryWithRetry(_retryPolicy);
							}
							Retry(sqlTransaction.Commit);
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}
	
		
		private SqlCommand CreateUpdateCancellingResultCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			var updateJobSetResultCommandText = "UPDATE [Stardust].[Job] " +
			                                    "SET Result = @Result " +
			                                    "WHERE JobId = @JobId";

			var updateCommand = new SqlCommand(updateJobSetResultCommandText,sqlConnection);
			updateCommand.Transaction = sqlTransaction;
			updateCommand.Parameters.AddWithValue("@JobId", jobId);
			updateCommand.Parameters.AddWithValue("@Result", "Cancelling...");

			return updateCommand;
		}

		private void CancelJobByJobIdWorker(Guid jobId, IHttpSender httpSender)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						var sentToWorkerNodeUri = string.Empty;

						using (var createSelectWorkerNodeUriCommand = CreateCommandHelper.CreateSelectWorkerNodeCommand(jobId, sqlConnection, sqlTransaction))
						{
							using (var selectSqlReader = createSelectWorkerNodeUriCommand.ExecuteReaderWithRetry(_retryPolicyTimeout))
							{
								if (selectSqlReader.HasRows)
								{
									selectSqlReader.Read();
									sentToWorkerNodeUri = selectSqlReader.GetString(selectSqlReader.GetOrdinal("SentToWorkerNodeUri"));
								}
							}
						}
						
						var taskSendCancel = new Task<HttpResponseMessage>(() =>
						{
							var builderHelper = new NodeUriBuilderHelper(sentToWorkerNodeUri);
							var uriCancel = builderHelper.GetCancelJobUri(jobId);

							return httpSender.DeleteAsync(uriCancel).Result;
						});

						taskSendCancel.Start();
						taskSendCancel.Wait();

						if (taskSendCancel.IsCompleted &&
						    taskSendCancel.Result.IsSuccessStatusCode)
						{
							using (var createUpdateCancellingResultCommand = CreateUpdateCancellingResultCommand(jobId, sqlConnection, sqlTransaction))
							{
								createUpdateCancellingResultCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
							}
							using (var deleteFromJobDefinitionsCommand = CreateDeleteFromJobQueueCommand(jobId, sqlConnection, sqlTransaction))
							{
								deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);
							}

							Retry(sqlTransaction.Commit);
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		




		private SqlCommand CreateSelectAllItemsInJobQueueSqlCommand(SqlConnection connection)
		{
			const string selectAllItemsInJobQueueCommandText =
				@"SELECT 
										[JobId],
										[Name],
										[Type],
										[Serialized],
										[CreatedBy],
										[Created]
									FROM [Stardust].[JobQueue]";

			var command = new SqlCommand(selectAllItemsInJobQueueCommandText);
			command.Connection = connection;

			return command;
		}
		
		private SqlCommand CreateDeleteFromJobQueueCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			const string deleteItemFromJobQueueItemCommandText =
					"DELETE FROM [Stardust].[JobQueue] " +
					"WHERE JobId = @JobId";

			var deleteFromJobQueueCommand = new SqlCommand(deleteItemFromJobQueueItemCommandText, sqlConnection);
			deleteFromJobQueueCommand.Parameters.AddWithValue("@JobId", jobId);
			deleteFromJobQueueCommand.Transaction = sqlTransaction;

			return deleteFromJobQueueCommand;
		}


		
		

		private void AssignJobToWorkerNodeWorker(IHttpSender httpSender,
		                                         Uri availableNode)
		{
			if (httpSender == null)
			{
				return;
			}

			if (availableNode == null)
			{
				return;
			}

			var selectOneJobQueueItemCommandText =
				@"SELECT Top 1  [JobId],
								[Name],
								[Created],
								[Type],
								[Serialized],
								[CreatedBy]
				FROM [Stardust].[JobQueue] 
				ORDER BY Created";

			var insertIntoJobCommandText = @"INSERT INTO [Stardust].[Job]
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


			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicyTimeout);

					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						var selectTop1FromJobDefinitionsCommand =
							new SqlCommand(selectOneJobQueueItemCommandText, sqlConnection)
							{
								Transaction = sqlTransaction
							};

						var reader =
							selectTop1FromJobDefinitionsCommand.ExecuteReaderWithRetry(_retryPolicyTimeout);

						if (!reader.HasRows)
						{
							reader.Close();
							reader.Dispose();

							sqlConnection.Close();

							return;
						}

						reader.Read();

						var jobId = reader.GetGuid(reader.GetOrdinal("JobId"));
						var name = reader.GetString(reader.GetOrdinal("Name"));
						var type = reader.GetString(reader.GetOrdinal("Type"));
						var serialized = reader.GetString(reader.GetOrdinal("Serialized"));
						var created = reader.GetDateTime(reader.GetOrdinal("Created"));
						var createdBy = reader.GetString(reader.GetOrdinal("CreatedBy"));

						var jobDefinition = new JobQueueItem
						{
							JobId = jobId,
							Name = name,
							Serialized = serialized.Replace(@"\", @""),
							Type = type,
							CreatedBy = createdBy,
							Created = created
						};

						reader.Close();
						reader.Dispose();

						var taskPostJob = new Task<HttpResponseMessage>(() =>
						{
							var builderHelper = new NodeUriBuilderHelper(availableNode);
							var urijob = builderHelper.GetJobTemplateUri();

							var response =
								httpSender.PostAsync(urijob, jobDefinition).Result;

							response.Content = new StringContent(availableNode.ToString());

							return response;
						});

						taskPostJob.Start();
						taskPostJob.Wait();

						string sentToWorkerNodeUri = null;

						if (taskPostJob.IsCompleted)
						{
							//Should we really ignore all other results?
							if (taskPostJob.Result.IsSuccessStatusCode ||
							    taskPostJob.Result.StatusCode.Equals(HttpStatusCode.BadRequest))
							{
								sentToWorkerNodeUri = taskPostJob.Result.Content.ContentToString();

								var commandInsertIntoJobHistory = new SqlCommand(insertIntoJobCommandText, sqlConnection)
								{
									Transaction = sqlTransaction
								};

								commandInsertIntoJobHistory.Parameters.AddWithValue("@JobId", jobId);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@Name", name);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@Type", type);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@Serialized", serialized);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@Started", DateTime.UtcNow);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@SentToWorkerNodeUri", sentToWorkerNodeUri);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@CreatedBy", createdBy);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@Created", created);
								commandInsertIntoJobHistory.Parameters.AddWithValue("@Ended", DBNull.Value);

								if (taskPostJob.Result.IsSuccessStatusCode)
								{
									commandInsertIntoJobHistory.Parameters.AddWithValue("@Result", DBNull.Value);
								}
								else
								{
									commandInsertIntoJobHistory.Parameters.AddWithValue("@Result", taskPostJob.Result.ReasonPhrase);
								}
								commandInsertIntoJobHistory.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

								var deleteItemFromJobQueueCommandText =
									new SqlCommand("DELETE FROM [Stardust].[JobQueue] " +
									               "WHERE JobId = @JobId", sqlConnection)
									{
										Transaction = sqlTransaction
									};

								deleteItemFromJobQueueCommandText.Parameters.AddWithValue("@JobId", jobId);

								deleteItemFromJobQueueCommandText.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
							}

							Retry(sqlTransaction.Commit);

							if (sentToWorkerNodeUri != null)
							{
								var builderHelper = new NodeUriBuilderHelper(sentToWorkerNodeUri);
								var urijob = builderHelper.GetUpdateJobUri(jobId);

								//what should happen if the response is not 200? 
								var resp = httpSender.PutAsync(urijob, null);
							}
						}
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
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


		private Job CreateJobFromSqlDataReader(SqlDataReader sqlDataReader)
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

		private JobQueueItem CreateJobQueueItemFromSqlDataReader(SqlDataReader sqlDataReader)
		{
			var jobQueueItem = new JobQueueItem
			{
				JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
				Name = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Name")),
				Serialized = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Serialized")),
				Type = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Type")),
				CreatedBy = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("CreatedBy")),
				Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created"))
			};

			return jobQueueItem;
		}

		public bool DoesJobQueueItemExists(Guid jobId)
		{
			return DoesItemExistsTemplateMethod(jobId,
												DoesJobQueueItemExistsWorker);
		}

		public bool DoesJobItemExists(Guid jobId)
		{
			return DoesItemExistsTemplateMethod(jobId,
												DoesJobItemExistsWorker);
		}

		public bool DoesJobDetailItemExists(Guid jobId)
		{
			return DoesItemExistsTemplateMethod(jobId,
												DoesJobDetailItemExistsWorker);
		}

		private bool DoesItemExistsTemplateMethod(Guid jobId,
										  Func<Guid, SqlConnection, bool> func)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					return func(jobId, sqlConnection);
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		private bool DoesJobDetailItemExistsWorker(Guid jobId, SqlConnection sqlConnection)
		{
			var command = CreateCommandHelper.CreateDoesJobDetailItemExistsCommand(jobId, sqlConnection);
			var count = Convert.ToInt32(command.ExecuteScalar());

			return count == 1;
		}

		private bool DoesJobQueueItemExistsWorker(Guid jobId, SqlConnection sqlConnection)
		{
			var command = CreateCommandHelper.CreateDoesJobQueueItemExistsCommand(jobId, sqlConnection);
			var count = Convert.ToInt32(command.ExecuteScalar());

			return count == 1;
		}

		private bool DoesJobItemExistsWorker(Guid jobId, SqlConnection sqlConnection)
		{
			var command = CreateCommandHelper.CreateDoesJobItemExistsCommand(jobId, sqlConnection);
			var count = Convert.ToInt32(command.ExecuteScalar());

			return count == 1;
		}


		private void Retry(Action action, int numerOfTries = 10)
		{
			var count = numerOfTries;

			var delay = TimeSpan.FromSeconds(1);

			while (true)
			{
				try
				{
					action();

					return;
				}

				catch (SqlException e)
				{
					--count;

					if (count <= 0)
					{
						throw;
					}

					if (e.Number == -2)
					{
						this.Log().Debug("Time out will retry.");
					}
					else
					{
						if (e.Number == 1205)
						{
							this.Log().Debug("Deadlock will retry.");
						}
						else
						{
							throw;
						}
					}

					Thread.Sleep(delay);
				}
			}
		}
	}
}