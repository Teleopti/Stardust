using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

		private readonly RetryPolicy _retryPolicy;

		private readonly CreateSqlCommandHelper _createSqlCommandHelper;

		public JobRepository(ManagerConfiguration managerConfiguration,
		                     RetryPolicyProvider retryPolicyProvider,
							 CreateSqlCommandHelper createSqlCommandHelper)
		{
			if (retryPolicyProvider == null)
			{
				throw new ArgumentNullException("retryPolicyProvider");
			}

			if (retryPolicyProvider.GetPolicy() == null)
			{
				throw new ArgumentNullException("retryPolicyProvider.GetPolicy");
			}

			_connectionString = managerConfiguration.ConnectionString;
			_createSqlCommandHelper = createSqlCommandHelper;

			_retryPolicy = retryPolicyProvider.GetPolicy();
		}

		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			try
			{
				Monitor.Enter(_lockAddItemToJobQueue);

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var sqlCommand = _createSqlCommandHelper.CreateInsertIntoJobQueueCommand(jobQueueItem, sqlConnection, null))
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
					using (var sqlCommand = _createSqlCommandHelper.CreateSelectAllItemsInJobQueueCommand(sqlConnection))
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
					using (var deleteFromJobQueueCommand = _createSqlCommandHelper.CreateDeleteFromJobQueueCommand(jobId, sqlConnection, null))
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
				var allAliveWorkerNodesUri = new List<Uri>();

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var selectAllAliveWorkerNodesCommand = _createSqlCommandHelper.CreateSelectAllAliveWorkerNodesCommand(sqlConnection))
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

				using (var updateResultCommand = _createSqlCommandHelper.CreateUpdateResultCommand(jobId, result, ended, sqlConnection))
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

				using (var insertCommand = _createSqlCommandHelper.CreateInsertIntoJobDetailCommand(jobId, detail, created, sqlConnection))
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

					using (var sqlSelectCommand = _createSqlCommandHelper.CreateGetJobQueueItemByJobIdCommand(jobId, sqlConnection))
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
					using (var selectJobByJobIdCommand = _createSqlCommandHelper.CreateSelectJobByJobIdCommand(jobId, sqlConnection))
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
					using (var getAllJobsCommand = _createSqlCommandHelper.CreateGetAllJobsCommand(sqlConnection))
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

					using (var getAllExecutingJobsCommand = _createSqlCommandHelper.CreateGetAllExecutingJobsCommand(sqlConnection))
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
					using (var selectJobDetailByJobIdCommand = _createSqlCommandHelper.CreateSelectJobDetailByJobIdCommand(jobId, sqlConnection))
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
						using (var selectJobThatDidNotEndCommand = _createSqlCommandHelper.CreateSelectJobThatDidNotEndCommand(workerNodeUri, sqlConnection, sqlTransaction))
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

							using (var insertIntojobQueueCommand = _createSqlCommandHelper.CreateInsertIntoJobQueueCommand(jobQueueItem, sqlConnection, sqlTransaction))
							{
								insertIntojobQueueCommand.ExecuteNonQueryWithRetry(_retryPolicy);
							}
							using (var deleteJobByJobIdCommand = _createSqlCommandHelper.CreateDeleteJobByJobIdCommand(jobQueueItem.JobId, sqlConnection, sqlTransaction))
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
	

		private void CancelJobByJobIdWorker(Guid jobId, IHttpSender httpSender)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						string sentToWorkerNodeUri = null;

						using (var createSelectWorkerNodeUriCommand = _createSqlCommandHelper.CreateSelectWorkerNodeCommand(jobId, sqlConnection, sqlTransaction))
						{
							using (var selectSqlReader = createSelectWorkerNodeUriCommand.ExecuteReaderWithRetry(_retryPolicy))
							{
								if (selectSqlReader.HasRows)
								{
									selectSqlReader.Read();
									sentToWorkerNodeUri = selectSqlReader.GetString(selectSqlReader.GetOrdinal("SentToWorkerNodeUri"));
								}
							}
						}

						if (sentToWorkerNodeUri != null)
						{

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
								using (var createUpdateCancellingResultCommand = _createSqlCommandHelper.CreateUpdateCancellingResultCommand(jobId, sqlConnection, sqlTransaction))
								{
									createUpdateCancellingResultCommand.ExecuteNonQueryWithRetry(_retryPolicy);
								}
								using (var deleteFromJobDefinitionsCommand = _createSqlCommandHelper.CreateDeleteFromJobQueueCommand(jobId, sqlConnection, sqlTransaction))
								{
									deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);
								}

								Retry(sqlTransaction.Commit);
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


		private void AssignJobToWorkerNodeWorker(IHttpSender httpSender, Uri availableNode)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					JobQueueItem jobQueueItem = null;

					using (var selectJobQueueItemCommand = new SqlCommand("AcquireQueuedJob", sqlConnection))
					{
						selectJobQueueItemCommand.CommandType = CommandType.StoredProcedure;

						SqlParameter retVal = new SqlParameter("@idd", SqlDbType.UniqueIdentifier);
						retVal.Direction = ParameterDirection.ReturnValue;
						selectJobQueueItemCommand.Parameters.Add(retVal);

						using (var reader = selectJobQueueItemCommand.ExecuteReaderWithRetry(_retryPolicy))
						{
							if (reader.HasRows)
							{
								reader.Read();
								jobQueueItem = CreateJobQueueItemFromSqlDataReader(reader);
							}
						}
					}
					if (jobQueueItem == null)
					{
						sqlConnection.Close();
						return;
					}

					var taskPostJob = new Task<HttpResponseMessage>(() =>
					{
						var builderHelper = new NodeUriBuilderHelper(availableNode);
						var urijob = builderHelper.GetJobTemplateUri();

						var response = httpSender.PostAsync(urijob, jobQueueItem).Result;
						response.Content = new StringContent(availableNode.ToString());

						return response;
					});

					taskPostJob.Start();
					taskPostJob.Wait();

					if (taskPostJob.IsCompleted)
					{
						if (taskPostJob.Result.IsSuccessStatusCode ||
						    taskPostJob.Result.StatusCode.Equals(HttpStatusCode.BadRequest))
						{
							string sentToWorkerNodeUri = taskPostJob.Result.Content.ReadAsStringAsync().Result;
							using (var sqlTransaction = sqlConnection.BeginTransaction())
							{
								using (var insertIntoJobCommand = _createSqlCommandHelper.CreateInsertIntoJobCommand(jobQueueItem, sentToWorkerNodeUri, sqlConnection, sqlTransaction))
								{
									if (taskPostJob.Result.IsSuccessStatusCode)
									{
										insertIntoJobCommand.Parameters.AddWithValue("@Result", DBNull.Value);
									}
									else
									{
										insertIntoJobCommand.Parameters.AddWithValue("@Result", taskPostJob.Result.ReasonPhrase);
									}
									insertIntoJobCommand.ExecuteNonQueryWithRetry(_retryPolicy);
								}
								using (var deleteJobQueueItemCommand = _createSqlCommandHelper.CreateDeleteFromJobQueueCommand(jobQueueItem.JobId, sqlConnection, sqlTransaction))
								{
									deleteJobQueueItemCommand.ExecuteNonQueryWithRetry(_retryPolicy);
								}
								Retry(sqlTransaction.Commit);
							}

							if (sentToWorkerNodeUri != null)
							{
								var builderHelper = new NodeUriBuilderHelper(sentToWorkerNodeUri);
								var urijob = builderHelper.GetUpdateJobUri(jobQueueItem.JobId);

								//what should happen if this response is not 200? 
								var resp = httpSender.PutAsync(urijob, null);
							}
						}
						else
						{
							using (var sqlTransaction = sqlConnection.BeginTransaction())
							{
								var commandText = "update [Stardust].[JobQueue] set Tagged = NULL where JobId = @Id";

								using (var cmd = new SqlCommand(commandText, sqlConnection, sqlTransaction))
								{
									cmd.Parameters.AddWithValue("@Id", jobQueueItem.JobId);
									cmd.ExecuteNonQuery();
								}
								sqlTransaction.Commit();
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
				Serialized = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Serialized")).Replace(@"\", @""),
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
			var command = _createSqlCommandHelper.CreateDoesJobDetailItemExistsCommand(jobId, sqlConnection);
			var count = Convert.ToInt32(command.ExecuteScalar());

			return count == 1;
		}

		private bool DoesJobQueueItemExistsWorker(Guid jobId, SqlConnection sqlConnection)
		{
			var command = _createSqlCommandHelper.CreateDoesJobQueueItemExistsCommand(jobId, sqlConnection);
			var count = Convert.ToInt32(command.ExecuteScalar());

			return count == 1;
		}

		private bool DoesJobItemExistsWorker(Guid jobId, SqlConnection sqlConnection)
		{
			var command = _createSqlCommandHelper.CreateDoesJobItemExistsCommand(jobId, sqlConnection);
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