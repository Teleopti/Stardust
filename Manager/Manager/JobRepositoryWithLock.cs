using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
	public class JobRepositoryWithLock : IJobRepository
	{
		private const string Added = "Added";
		private const string Started = "Started";
		private const string Deleted = "Deleted";

		private const string Canceling = "Canceling...";

		private readonly string _connectionString;

		private readonly object _lockAddItemToJobQueue = new object();

		private readonly object _lockTryAssignJobToWorkerNode = new object();

		private readonly RetryPolicy _retryPolicy;
		private readonly RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts> _retryPolicyTimeout;

		/// <summary>
		///     Constructor.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="retryPolicyProvider"></param>
		public JobRepositoryWithLock(string connectionString,
		                               RetryPolicyProvider retryPolicyProvider)
		{
			_connectionString = connectionString;
			_retryPolicy = retryPolicyProvider.GetPolicy();
			_retryPolicyTimeout = retryPolicyProvider.GetPolicyWithTimeout();
		}

		public void AddItemToJobQueue(JobQueueItem jobQueueItem)
		{
			const string insertIntoJobQueueCommandText =
				@"INSERT INTO [Stardust].[JobQueue]
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


			try
			{
				Monitor.Enter(_lockAddItemToJobQueue);

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var sqlCommand =
						new SqlCommand(insertIntoJobQueueCommandText, sqlConnection);

					sqlCommand.Parameters.AddWithValue("@JobId", jobQueueItem.JobId);
					sqlCommand.Parameters.AddWithValue("@Name", jobQueueItem.Name);
					sqlCommand.Parameters.AddWithValue("@Type", jobQueueItem.Type);
					sqlCommand.Parameters.AddWithValue("@Serialized", jobQueueItem.Serialized);
					sqlCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
					sqlCommand.Parameters.AddWithValue("@CreatedBy", jobQueueItem.CreatedBy);

					sqlCommand.ExecuteNonQueryWithRetry(_retryPolicy);
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
			const string selectAllItemsInJobQueueCommandText =
									@"SELECT 
										[JobId],
										[Name],
										[Type],
										[Serialized],
										[CreatedBy],
										[Created],
									FROM [Stardust].[JobQueue]";

			var listToReturn = new List<JobQueueItem>();

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var sqlCommand =
						new SqlCommand(selectAllItemsInJobQueueCommandText, sqlConnection);

					var sqlDataReader = sqlCommand.ExecuteReaderWithRetry(_retryPolicy);

					if (!sqlDataReader.HasRows)
					{
						sqlDataReader.Close();
						sqlConnection.Close();

						return listToReturn;
					}

					var ordinalPositionForIdField = sqlDataReader.GetOrdinal("JobId");
					var ordinalPositionForNameField = sqlDataReader.GetOrdinal("Name");
					var ordinalPositionForSerializedField = sqlDataReader.GetOrdinal("Serialized");
					var ordinalPositionForTypeField = sqlDataReader.GetOrdinal("Type");
					var ordinalPositionForCreatedByField = sqlDataReader.GetOrdinal("CreatedBy");
					var ordinalPositionForCreatedField = sqlDataReader.GetOrdinal("Created");

					while (sqlDataReader.Read())
					{
						var jobDefinition = new JobQueueItem
						{
							JobId = sqlDataReader.GetGuid(ordinalPositionForIdField),
							Name = sqlDataReader.GetNullableString(ordinalPositionForNameField),
							Serialized = sqlDataReader.GetNullableString(ordinalPositionForSerializedField),
							Type = sqlDataReader.GetNullableString(ordinalPositionForTypeField),
							CreatedBy = sqlDataReader.GetNullableString(ordinalPositionForCreatedByField),
							Created = sqlDataReader.GetDateTime(ordinalPositionForCreatedField)
						};

						listToReturn.Add(jobDefinition);
					}

					sqlDataReader.Close();
					sqlDataReader.Dispose();

					sqlConnection.Close();
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
				throw;
			}

			return listToReturn;
		}

		public void DeleteItemInJobQueueByJobId(Guid jobId)
		{
			const string deleteItemFromJobQueueItemCommandText =
								"DELETE FROM [Stardust].[JobQueue] " +
								"WHERE JobId = @JobId";

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var deleteFromJobDefinitionsCommand =
						new SqlCommand(deleteItemFromJobQueueItemCommandText, sqlConnection);

					deleteFromJobDefinitionsCommand.Parameters.AddWithValue("@JobId", jobId);

					deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}

		public void FreeJobIfNodeIsAssigned(string url)
		{
		}

		public virtual void TryAssignJobToWorkerNode(IHttpSender httpSender)
		{
			var selectAllAliveWorkerNodesCommandText =
				@"SELECT   [Id]
						  ,[Url]
						  ,[Heartbeat]
						  ,[Alive]
					  FROM [Stardust].[WorkerNode]
					  where Alive = 1";

			var selectOneJobQueueItemCommandText =
				@"SELECT Top 1  [JobId],
								[Name],
								[Created],
								[Type],
								[Serialized],
								[CreatedBy]
				FROM [Stardust].[JobQueue]";

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
				Monitor.Enter(_lockTryAssignJobToWorkerNode);

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicyTimeout);

					// --------------------------------------------------
					// Get all alive worker nodes.
					// --------------------------------------------------
					var selectAllAliveWorkerNodesCommand =
						new SqlCommand(selectAllAliveWorkerNodesCommandText, sqlConnection);

					var readerAliveWorkerNodes = selectAllAliveWorkerNodesCommand.ExecuteReader();

					if (!readerAliveWorkerNodes.HasRows)
					{
						readerAliveWorkerNodes.Close();
						sqlConnection.Close();

						return;
					}

					var allAliveWorkerNodesUri = new List<Uri>();

					var ordinalPosForUrl = readerAliveWorkerNodes.GetOrdinal("Url");

					while (readerAliveWorkerNodes.Read())
					{
						allAliveWorkerNodesUri.Add(new Uri(readerAliveWorkerNodes.GetString(ordinalPosForUrl)));
					}

					readerAliveWorkerNodes.Close();
					readerAliveWorkerNodes.Dispose();

					// --------------------------------------------------
					// Select one job defintion.
					// --------------------------------------------------
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

						//------------------------------------------------
						// POST message.
						//------------------------------------------------
						var taskPostJob = new Task<HttpResponseMessage>(() =>
						{
							foreach (var workerNodeUri in allAliveWorkerNodesUri)
							{
								var builderHelper = new NodeUriBuilderHelper(workerNodeUri);
								var urijob = builderHelper.GetJobTemplateUri();

								var response =
									httpSender.PostAsync(urijob, jobDefinition).Result;

								if (!response.IsSuccessStatusCode) continue;

								response.Content = new StringContent(workerNodeUri.ToString());

								return response;
							}

							return new HttpResponseMessage(HttpStatusCode.NotFound);
						});

						taskPostJob.Start();
						taskPostJob.Wait();

						if (taskPostJob.IsCompleted)
						{
							//--------------------------------------------
							// Success.
							//--------------------------------------------
							if (taskPostJob.Result.IsSuccessStatusCode)
							{
								//----------------------------------------
								// Insert into job.
								//----------------------------------------
								var sentToWorkerNodeUri = taskPostJob.Result.Content.ContentToString();

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
								commandInsertIntoJobHistory.Parameters.AddWithValue("@Result", DBNull.Value);

								commandInsertIntoJobHistory.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

								//----------------------------------------
								// Delete from job queue.
								//----------------------------------------
								var deleteItemFromJobQueueCommandText =
									new SqlCommand("DELETE FROM [Stardust].[JobQueue] " +
									               "WHERE JobId = @JobId", sqlConnection)
									{
										Transaction = sqlTransaction
									};

								deleteItemFromJobQueueCommandText.Parameters.AddWithValue("@JobId", jobId);

								deleteItemFromJobQueueCommandText.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
							}
							else
							{
								//--------------------------------------------
								// Bad request.
								//--------------------------------------------
								if (taskPostJob.Result.StatusCode.Equals(HttpStatusCode.BadRequest))
								{
								}
							}
						}

						sqlTransaction.Commit();
					}
				}
			}

			catch (SqlException exp)
			{
				if (exp.Number == -2) //Timeout
				{
					this.Log().InfoWithLineNumber(exp.Message);
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

			finally
			{
				Monitor.Exit(_lockTryAssignJobToWorkerNode);
			}
		}

		public void CancelJobByJobId(Guid jobId, IHttpSender httpSender)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicyTimeout);

					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						//--------------------------------------------
						// Select worker node uri from job.
						//--------------------------------------------
						var selectWorkerNodeUriFromJobCommandText = "SELECT SentToWorkerNodeUri " +
						                                            "FROM [Stardust].[Job] " +
						                                            "WHERE JobId = @JobId";

						var selectCommand = new SqlCommand(selectWorkerNodeUriFromJobCommandText, sqlConnection)
						{
							Transaction = sqlTransaction
						};

						selectCommand.Parameters.AddWithValue("@JobId", jobId);

						var selectSqlReader = selectCommand.ExecuteReaderWithRetry(_retryPolicyTimeout);

						if (!selectSqlReader.HasRows)
						{
							selectSqlReader.Close();
							selectSqlReader.Dispose();
							sqlConnection.Close();

							return;
						}

						selectSqlReader.Read();

						var sentToWorkerNodeUri =
							selectSqlReader.GetString(selectSqlReader.GetOrdinal("SentToWorkerNodeUri"));

						selectSqlReader.Close();
						selectSqlReader.Dispose();

						//------------------------------------------------
						// Send cancel job.
						//------------------------------------------------
						var taskSendCancel = new Task<HttpResponseMessage>(() =>
						{
							var builderHelper = new NodeUriBuilderHelper(sentToWorkerNodeUri);

							var uriCancel = builderHelper.GetCancelJobUri(jobId);

							return httpSender.DeleteAsync(uriCancel).Result;
						});

						taskSendCancel.Start();
						taskSendCancel.Wait();

						if (taskSendCancel.IsCompleted && taskSendCancel.Result.IsSuccessStatusCode)
						{
							var updateJobSetResultCommandText = "UPDATE [Stardust].[Job] " +
							                                    "SET Result = @Result " +
							                                    "WHERE JobId = @JobId";

							var updateCommand = new SqlCommand(updateJobSetResultCommandText, sqlConnection)
							{
								Transaction = sqlTransaction
							};

							updateCommand.Parameters.AddWithValue("@JobId", jobId);
							updateCommand.Parameters.AddWithValue("@Result", Canceling);

							updateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
						}

						sqlTransaction.Commit();
					}

					sqlConnection.Close();
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}

		public void SetEndResultForJob(Guid jobId, string result, DateTime ended)
		{
			var updateJobCommandText = "UPDATE [Stardust].[Job] " +
			                           "SET Result = @Result," +
			                           "Ended = @Ended " +
			                           "WHERE JobId = @JobId";

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicyTimeout);

				var updateCommand = new SqlCommand(updateJobCommandText, sqlConnection);

				updateCommand.Parameters.AddWithValue("@JobId", jobId);
				updateCommand.Parameters.AddWithValue("@Result", result);
				updateCommand.Parameters.AddWithValue("@Ended", ended);

				updateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
			}
		}

		public void ReportProgress(Guid jobId,
		                           string detail,
		                           DateTime created)
		{
			var insertIntoJobCommandText = @"INSERT INTO [Stardust].[JobDetail]
													   ([JobId]
													   ,[Created]
													   ,[Detail])
												 VALUES
														(@JobId
													   ,@Created
													   ,@Detail)";

			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				sqlConnection.OpenWithRetry(_retryPolicyTimeout);

				var insertCommand =
					new SqlCommand(insertIntoJobCommandText, sqlConnection);

				insertCommand.Parameters.AddWithValue("@JobId", jobId);
				insertCommand.Parameters.AddWithValue("@Detail", detail);
				insertCommand.Parameters.AddWithValue("@Created", created);

				insertCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
			}
		}

		public Job GetJobByJobId(Guid jobId)
		{
			var selectJobByJobIdCommandText = @"SELECT [JobId]
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

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicyTimeout);

					var selectCommand =
						new SqlCommand(selectJobByJobIdCommandText, sqlConnection);

					selectCommand.Parameters.AddWithValue("@JobId", jobId);

					using (var sqlDataReader =
						selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							sqlDataReader.Read();

							var job = new Job
							{
								JobId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("JobId")),
								Name = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Name")),
								SentToWorkerNodeUri = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("SentToWorkerNodeUri")),
								Type = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Type")),
								CreatedBy = sqlDataReader.GetString(sqlDataReader.GetOrdinal("CreatedBy")),
								Result = sqlDataReader.GetNullableString(sqlDataReader.GetOrdinal("Result")),
								Created = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Created")),
								Started = sqlDataReader.GetNullableDateTime(sqlDataReader.GetOrdinal("Started")),
								Ended = sqlDataReader.GetNullableDateTime(sqlDataReader.GetOrdinal("Ended"))
							};

							sqlDataReader.Close();

							return job;
						}
					}
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return null;
		}


		public IList<Job> GetAllJobs()
		{
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
										  FROM [Stardust].[Job]";

			try
			{
				var jobs = new List<Job>();

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var selectCommand = new SqlCommand(selectCommandText, sqlConnection);

					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							var ordinalPosForJobId = sqlDataReader.GetOrdinal("JobId");
							var ordinalPosForName = sqlDataReader.GetOrdinal("Name");
							var ordinalPosForType = sqlDataReader.GetOrdinal("Type");
							var ordinalPosForResult = sqlDataReader.GetOrdinal("Result");

							var ordinalPosForSentToWorkerNodeUri = sqlDataReader.GetOrdinal("SentToWorkerNodeUri");

							var ordinalPosForStarted = sqlDataReader.GetOrdinal("Started");
							var ordinalPosForEnded = sqlDataReader.GetOrdinal("Ended");

							var ordinalPosForCreatedBy = sqlDataReader.GetOrdinal("CreatedBy");
							var ordinalPosForCreated = sqlDataReader.GetOrdinal("Created");

							while (sqlDataReader.Read())
							{
								var job = new Job
								{
									JobId = sqlDataReader.GetGuid(ordinalPosForJobId),
									Name = sqlDataReader.GetNullableString(ordinalPosForName),
									SentToWorkerNodeUri = sqlDataReader.GetNullableString(ordinalPosForSentToWorkerNodeUri),
									Type = sqlDataReader.GetNullableString(ordinalPosForType),
									CreatedBy = sqlDataReader.GetString(ordinalPosForCreatedBy),
									Result = sqlDataReader.GetNullableString(ordinalPosForResult),
									Created = sqlDataReader.GetDateTime(ordinalPosForCreated),
									Started = sqlDataReader.GetNullableDateTime(ordinalPosForStarted),
									Ended = sqlDataReader.GetNullableDateTime(ordinalPosForEnded)
								};

								jobs.Add(job);
							}
						}

						sqlDataReader.Close();
						sqlConnection.Close();
					}

					return jobs;
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return null;
		}

		public IList<Job> GetAllExecutingJobs()
		{
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
										  FROM [ManagerDB].[Stardust].[Job]
										  WHERE [Started] IS NOT NULL AND [Ended] IS NULL";

			var jobs = new List<Job>();

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var selectCommand = new SqlCommand(selectCommandText, sqlConnection);

					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							var ordinalPosForJobId = sqlDataReader.GetOrdinal("JobId");
							var ordinalPosForName = sqlDataReader.GetOrdinal("Name");
							var ordinalPosForType = sqlDataReader.GetOrdinal("Type");
							var ordinalPosForResult = sqlDataReader.GetOrdinal("Result");

							var ordinalPosForSentToWorkerNodeUri = sqlDataReader.GetOrdinal("SentToWorkerNodeUri");

							var ordinalPosForStarted = sqlDataReader.GetOrdinal("Started");
							var ordinalPosForEnded = sqlDataReader.GetOrdinal("Ended");

							var ordinalPosForCreatedBy = sqlDataReader.GetOrdinal("CreatedBy");
							var ordinalPosForCreated = sqlDataReader.GetOrdinal("Created");

							while (sqlDataReader.Read())
							{
								var job = new Job
								{
									JobId = sqlDataReader.GetGuid(ordinalPosForJobId),
									Name = sqlDataReader.GetNullableString(ordinalPosForName),
									SentToWorkerNodeUri = sqlDataReader.GetNullableString(ordinalPosForSentToWorkerNodeUri),
									Type = sqlDataReader.GetNullableString(ordinalPosForType),
									CreatedBy = sqlDataReader.GetString(ordinalPosForCreatedBy),
									Result = sqlDataReader.GetNullableString(ordinalPosForResult),
									Created = sqlDataReader.GetDateTime(ordinalPosForCreated),
									Started = sqlDataReader.GetNullableDateTime(ordinalPosForStarted),
									Ended = sqlDataReader.GetNullableDateTime(ordinalPosForEnded)
								};

								jobs.Add(job);
							}
						}

						sqlDataReader.Close();
						sqlConnection.Close();
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return jobs;
		}

		public IList<Uri> GetAllDistinctSentToWorkerNodeUri()
		{
			const string selectCommandText = @"SELECT   
											   DISTINCT([SentToWorkerNodeUri])
											   FROM [ManagerDB].[Stardust].[Job]
											   WHERE SentToWorkerNodeUri IS NOT NULL";


			var listOfUri = new List<Uri>();

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var selectCommand = new SqlCommand(selectCommandText, sqlConnection);

					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							var ordinalPosForSentToWorkerNodeUri = sqlDataReader.GetOrdinal("SentToWorkerNodeUri");

							while (sqlDataReader.Read())
							{
								listOfUri.Add(new Uri(sqlDataReader.GetString(ordinalPosForSentToWorkerNodeUri)));
							}
						}

						sqlDataReader.Close();
						sqlConnection.Close();
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return listOfUri;
		}

		public IList<Uri> GetDistinctSentToWorkerNodeUriForExecutingJobs()
		{
			const string selectCommandText = @"SELECT   
											   DISTINCT([SentToWorkerNodeUri])
											   FROM [ManagerDB].[Stardust].[Job]
											   WHERE [SentToWorkerNodeUri] IS NOT NULL AND 
													 [Started] IS NOT NULL AND [Ended] IS NULL";


			var listOfUri = new List<Uri>();

			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var selectCommand = new SqlCommand(selectCommandText, sqlConnection);

					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							var ordinalPosForSentToWorkerNodeUri = sqlDataReader.GetOrdinal("SentToWorkerNodeUri");

							while (sqlDataReader.Read())
							{
								listOfUri.Add(new Uri(sqlDataReader.GetString(ordinalPosForSentToWorkerNodeUri)));
							}
						}

						sqlDataReader.Close();
						sqlConnection.Close();
					}
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return listOfUri;
		}

		public IList<JobDetail> GetJobDetailsByJobId(Guid id)
		{
			try
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
										FROM [Stardust].[JobDetail]  
										WHERE JobId = @JobId";

					var sqlCommand = new SqlCommand(selectCommandText);

					sqlCommand.Parameters.AddWithValue("@JobId", id);


					using (var sqlDataReader =
						sqlCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							var ordinalPositionForIdField = sqlDataReader.GetOrdinal("Id");
							var ordinalPositionForJobIdField = sqlDataReader.GetOrdinal("JobId");
							var ordinalPositionForCreatedField = sqlDataReader.GetOrdinal("Created");
							var ordinalPostionForDetailField = sqlDataReader.GetOrdinal("Detail");

							while (sqlDataReader.Read())
							{
								var detail = new JobDetail
								{
									Id = sqlDataReader.GetInt32(ordinalPositionForIdField),
									JobId = sqlDataReader.GetGuid(ordinalPositionForJobIdField),
									Created = sqlDataReader.GetDateTime(ordinalPositionForCreatedField),
									Detail = sqlDataReader.GetNullableString(ordinalPostionForDetailField)
								};

								jobDetails.Add(detail);
							}
						}

						sqlDataReader.Close();
						sqlConnection.Close();
					}

					return jobDetails;
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return null;
		}
	}
}