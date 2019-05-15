using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Newtonsoft.Json;
using Stardust.Manager.Extensions;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class JobRepository : IJobRepository
	{
		private readonly string _connectionString;
		private readonly RetryPolicy _retryPolicy;
		private readonly IHttpSender _httpSender;
		private readonly JobRepositoryCommandExecuter _jobRepositoryCommandExecuter;
		private readonly ILog ManagerLogger;
		private readonly object _requeueJobLock = new object();
		private readonly object _assigningJob = new object();

		public JobRepository(ManagerConfiguration managerConfiguration,
		                     RetryPolicyProvider retryPolicyProvider,
		                     IHttpSender httpSender, 
							 JobRepositoryCommandExecuter jobRepositoryCommandExecuter,
							ILog managerLogger)
		{
			if (retryPolicyProvider == null)
			{
				throw new ArgumentNullException(nameof(retryPolicyProvider));
			}

			if (retryPolicyProvider.GetPolicy() == null)
			{
				throw new ArgumentNullException("retryPolicyProvider.GetPolicy");
			}

			_connectionString = managerConfiguration.ConnectionString;
			_httpSender = httpSender;
			_jobRepositoryCommandExecuter = jobRepositoryCommandExecuter;
			ManagerLogger = managerLogger;

			_retryPolicy = retryPolicyProvider.GetPolicy();
		}

		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					_jobRepositoryCommandExecuter.InsertIntoJobQueue(jobQueueItem, sqlConnection);
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		public List<JobQueueItem> GetAllItemsInJobQueue()
		{
			try
			{
				List<JobQueueItem> listToReturn;
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					listToReturn = _jobRepositoryCommandExecuter.SelectAllItemsInJobQueue(sqlConnection);
				}
				return listToReturn;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}


		private void DeleteJobQueueItemByJobId(Guid jobId)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					_jobRepositoryCommandExecuter.DeleteJobFromJobQueue(jobId, sqlConnection);
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}


		public void AssignJobToWorkerNode()
		{
			try
			{
				ManagerLogger.Info("Assigning jobs to node");
				List<Uri> allAvailableWorkerNodes;
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					allAvailableWorkerNodes = _jobRepositoryCommandExecuter.SelectAllAvailableWorkerNodes(sqlConnection);
				}

				if (!allAvailableWorkerNodes.Any()) return;

				//sending to the available nodes
				ManagerLogger.Info(allAvailableWorkerNodes.Count + " nodes found that are available");
				var shuffledNodes = allAvailableWorkerNodes.OrderBy(a => Guid.NewGuid()).ToList();
				foreach (var nodeUri in shuffledNodes)
				{
					ManagerLogger.Info("trying to assign the job to node " + nodeUri);
					AssignJobToWorkerNodeWorker(nodeUri);
					Thread.Sleep(500);
				}
			}
			catch (SqlException exp)
			{
				if (exp.Message.Contains("PK_Job"))
				{
					// just skip it some other Manager picked it up just before I did
					this.Log().WarningWithLineNumber("Could not assign job to a Node. It was already assigned, probably by another Manager.");
				}
				else
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}

		public void CancelJobByJobId(Guid jobId)
		{
			try
			{
				if (DoesJobQueueItemExists(jobId))
				{
					DeleteJobQueueItemByJobId(jobId);
				}
				else
				{
					CancelJobByJobIdWorker(jobId);
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		public void UpdateResultForJob(Guid jobId, string result, DateTime ended)
		{
		
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				_jobRepositoryCommandExecuter.UpdateResult(jobId, result, ended, sqlConnection);

				var finishDetail = "Job finished";
				if (result == "Canceled")
				{
					finishDetail = "Job was canceled";
				}
				else if (result == "Fatal Node Failure" || result == "Failed")
				{
					finishDetail = "Job Failed";
				}

			 _jobRepositoryCommandExecuter.InsertJobDetail(jobId, finishDetail, sqlConnection);
			}
		}

		public void CreateJobDetailByJobId(Guid jobId, string detail, DateTime created)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				_jobRepositoryCommandExecuter.InsertJobDetail(jobId, detail, sqlConnection);
			}
		}



		public JobQueueItem GetJobQueueItemByJobId(Guid jobId)
		{
			try
			{
				JobQueueItem jobQueueItem;
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					jobQueueItem = _jobRepositoryCommandExecuter.SelectJobQueueItem(jobId, sqlConnection);
				}
				return jobQueueItem;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		public Job GetJobByJobId(Guid jobId)
		{
			try
			{
				Job job;
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					job = _jobRepositoryCommandExecuter.SelectJob(jobId, sqlConnection);
				}
				return job;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}


		public IList<Job> GetAllJobs()
		{
			try
			{
				
				var jobs = new List<Job>();
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					jobs = _jobRepositoryCommandExecuter.SelectAllJobs(sqlConnection);
				}
				return jobs;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}


		public IList<Job> GetAllExecutingJobs()
		{
			try
			{
				List<Job> jobs;
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					jobs = _jobRepositoryCommandExecuter.SelectAllExecutingJobs(sqlConnection);
				}
				return jobs;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}


		public IList<JobDetail> GetJobDetailsByJobId(Guid jobId)
		{
			try
			{
				List<JobDetail> jobDetails;
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					jobDetails = _jobRepositoryCommandExecuter.SelectJobDetails(jobId, sqlConnection);
				}
				return jobDetails;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}
		}

		public bool PingWorkerNode(Uri workerNodeUri)
		{
			var builderHelper = new NodeUriBuilderHelper(workerNodeUri);
			var urijob = builderHelper.GetPingTemplateUri();
			HttpResponseMessage response;
			try
			{
				response = _httpSender.GetAsync(urijob).Result;
			}
			catch (AggregateException)
			{
				return false;
			}

			return response != null && response.IsSuccessStatusCode ;
		}


		public void RequeueJobThatDidNotEndByWorkerNodeUri(string workerNodeUri)
		{
			try
			{
				lock (_requeueJobLock)
				{
					using (var sqlConnection = new SqlConnection(_connectionString))
					{
						sqlConnection.OpenWithRetry(_retryPolicy);
						using (var sqlTransaction = sqlConnection.BeginTransaction())
						{
							var job = _jobRepositoryCommandExecuter.SelectExecutingJob(workerNodeUri, sqlConnection, sqlTransaction);

							if (job == null) return;
							var jobQueueItem = new JobQueueItem
							{
								Created = job.Created,
								CreatedBy = job.CreatedBy,
								JobId = job.JobId,
								Serialized = job.Serialized,
								Name = job.Name,
								Type = job.Type,
								Policy = job.Policy
							};

							_jobRepositoryCommandExecuter.InsertIntoJobQueue(jobQueueItem, sqlConnection, sqlTransaction);
							_jobRepositoryCommandExecuter.DeleteJob(jobQueueItem.JobId, sqlConnection, sqlTransaction);
							sqlTransaction.Commit();
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


		private void CancelJobByJobIdWorker(Guid jobId)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);
					using (var sqlTransaction = sqlConnection.BeginTransaction())
					{
						var sentToWorkerNodeUri = _jobRepositoryCommandExecuter.SelectWorkerNode(jobId, sqlConnection, sqlTransaction);
						if (sentToWorkerNodeUri == null) return;

						var builderHelper = new NodeUriBuilderHelper(sentToWorkerNodeUri);
						var uriCancel = builderHelper.GetCancelJobUri(jobId);
						var response = _httpSender.DeleteAsync(uriCancel).GetAwaiter().GetResult();

						if (response != null && response.IsSuccessStatusCode)
						{
							_jobRepositoryCommandExecuter.UpdateResult(jobId,"Canceled", DateTime.UtcNow, sqlConnection, sqlTransaction);
							_jobRepositoryCommandExecuter.DeleteJobFromJobQueue(jobId, sqlConnection, sqlTransaction);

							sqlTransaction.Commit();
						}
						else
						{
							this.Log().ErrorWithLineNumber("Could not send cancel to node. JobId: " + jobId);
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

		private void AssignJobToWorkerNodeWorker(Uri availableNode)
		{
			ManagerLogger.Info("starting the assignment process");
			lock (_assigningJob)
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					var responseCodeOk = false;
					PrepareToStartJobResult result = null;
					JobQueueItem jobQueueItem = null;
					try
					{
						sqlConnection.OpenWithRetry(_retryPolicy);
						jobQueueItem = _jobRepositoryCommandExecuter.AcquireJobQueueItem(sqlConnection);
						if (jobQueueItem == null)
						{
							sqlConnection.Close();
							ManagerLogger.Info("no job acquired for node " + availableNode);
							return;
						}

						ManagerLogger.Info("acquired job with id  " + jobQueueItem.JobId + " for node " + availableNode);
						var builderHelper = new NodeUriBuilderHelper(availableNode);
						var urijob = builderHelper.GetJobTemplateUri();
						ManagerLogger.Info("posting the job to the node");
						HttpResponseMessage response = null;
						try
						{
							response = _httpSender.PostAsync(urijob, jobQueueItem).GetAwaiter().GetResult();
						}
						catch (HttpRequestException httpRequestException)
						{
							var nodeNotFound = httpRequestException.InnerException is
									WebException;

							if (!nodeNotFound) throw;
							ManagerLogger.Info($"Send job to node:{availableNode} failed. {httpRequestException.Message}", httpRequestException);
							return;
						}
						
						ManagerLogger.Info(response?.ReasonPhrase + " response from the node");

						if (response == null)
							result = new PrepareToStartJobResult();
						else
							result = JsonConvert.DeserializeObject<PrepareToStartJobResult>(response.Content.ReadAsStringAsync().Result);
						
						responseCodeOk = response != null &&
						                 (response.IsSuccessStatusCode || response.StatusCode.Equals(HttpStatusCode.BadRequest));
						
						if (responseCodeOk && result.IsAvailable)
						{
							var sentToWorkerNodeUri = availableNode.ToString();
							ManagerLogger.Info("node is ok fix the db now");
							using (var sqlTransaction = sqlConnection.BeginTransaction())
							{
								_jobRepositoryCommandExecuter.InsertIntoJob(jobQueueItem, sentToWorkerNodeUri, sqlConnection, sqlTransaction);
								_jobRepositoryCommandExecuter.DeleteJobFromJobQueue(jobQueueItem.JobId, sqlConnection, sqlTransaction);
								_jobRepositoryCommandExecuter.InsertJobDetail(jobQueueItem.JobId, "Job Started", sqlConnection, sqlTransaction);
								sqlTransaction.Commit();
							}

							if (!response.IsSuccessStatusCode) return;

							urijob = builderHelper.GetUpdateJobUri(jobQueueItem.JobId);
							//what should happen if this response is not 200? 
							ManagerLogger.Info("asking the node to start the job");
							_httpSender.PutAsync(urijob, null);
						}
						else
						{
							ManagerLogger.Info($"response from the node was not ok {response?.ReasonPhrase}. Node.IsAvailable={result.IsAvailable}");
						}
					}
					catch (Exception ex)
					{
						this.Log().Info($"Failed for node {availableNode} {ex.Message}");
						throw;
					}
					finally
					{
						if ((!responseCodeOk || !result.IsAvailable) && jobQueueItem != null)
						{
							using (var sqlTransaction = sqlConnection.BeginTransaction())
							{
								_jobRepositoryCommandExecuter.TagQueueItem(jobQueueItem.JobId, sqlConnection, sqlTransaction);
								sqlTransaction.Commit();
							}
						}
					}
				}
			}
		}

		public bool DoesJobQueueItemExists(Guid jobId)
		{
			int count;
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicy);
				count =_jobRepositoryCommandExecuter.SelectCountJobQueueItem(jobId, sqlConnection);
			}
			return count == 1;
		}
	}
}