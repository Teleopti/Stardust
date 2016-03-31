using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
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
		private readonly RetryPolicy _retryPolicy;
		private readonly RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts> _retryPolicyTimeout;

		public JobRepository(string connectionString,
		                     RetryPolicyProvider retryPolicyProvider)
		{
			_connectionString = connectionString;
			_retryPolicy = retryPolicyProvider.GetPolicy();
			_retryPolicyTimeout = retryPolicyProvider.GetPolicyWithTimeout();
		}

		public void Add(JobDefinition jobDefinition)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				try
				{
					//-------------------------------------------------------------
					// Open sql connection.
					//-------------------------------------------------------------
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var sqlTransaction = sqlConnection.BeginTransaction())
					{
						//-------------------------------------------------------------
						// Insert into job definitions.
						//-------------------------------------------------------------
						var insertIntoJobDefinitionsCommand =
							CreateInsertIntoJobDefinitionsCommand(jobDefinition);

						insertIntoJobDefinitionsCommand.Connection = sqlConnection;
						insertIntoJobDefinitionsCommand.Transaction = sqlTransaction;
						insertIntoJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						//-------------------------------------------------------------
						// Insert into job history.
						//-------------------------------------------------------------
						var insertIntoJobHistoryCommand =
							CreateInsertIntoJobHistoryCommand(jobDefinition);

						insertIntoJobHistoryCommand.Connection = sqlConnection;
						insertIntoJobHistoryCommand.Transaction = sqlTransaction;
						insertIntoJobHistoryCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						//-------------------------------------------------------------
						//Insert into job history detail. 
						//-------------------------------------------------------------
						var insertIntoJobHistoryDetailCommand =
							CreateInsertIntoJobHistoryDetailCommand(jobDefinition.Id, "Added", DateTime.UtcNow);

						insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
						insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;
						insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						//-------------------------------------------------------------
						// Commit transaction.
						//-------------------------------------------------------------
						sqlTransaction.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					sqlConnection.Close();
				}
			}
		}

		public List<JobDefinition> LoadAll()
		{
			try
			{
				var listToReturn = new List<JobDefinition>();

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var selectAllFromJobDefinitionsCommand = CreateSelectAllFromJobDefinitionsCommand();
					selectAllFromJobDefinitionsCommand.Connection = sqlConnection;

					var sqlDataReader =
						selectAllFromJobDefinitionsCommand.ExecuteReaderWithRetry(_retryPolicy);

					if (sqlDataReader.HasRows)
					{
						while (sqlDataReader.Read())
						{
							var jobDefinition = new JobDefinition
							{
								Id = (Guid) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Id")),
								Name = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Name")),
								Serialized = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Serialized")),
								Type = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Type")),
								UserName = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("UserName")),
								AssignedNode = GetValue(sqlDataReader.GetValue(sqlDataReader.GetOrdinal("AssignedNode"))),
								Status = GetValue(sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Status")))
							};

							listToReturn.Add(jobDefinition);
						}
					}
					sqlDataReader.Close();
					sqlConnection.Close();
				}
				return listToReturn;
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return null;
		}

		public void DeleteJob(Guid jobId)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				try
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var sqlTransaction = sqlConnection.BeginTransaction())
					{
						//-----------------------------------------------
						// Delete job definition.
						//-----------------------------------------------
						var deleteFromJobDefinitionsCommand =
							CreateDeleteFromJobDefinitionsCommand(jobId);

						deleteFromJobDefinitionsCommand.Connection = sqlConnection;
						deleteFromJobDefinitionsCommand.Transaction = sqlTransaction;
						deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						sqlTransaction.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					sqlConnection.Close();
				}
			}
		}

		public void FreeJobIfNodeIsAssigned(string url)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				try
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var sqlTransaction = sqlConnection.BeginTransaction())
					{
						var sqlCommand =
							CreateUpdateJobDefinitionsClearAssignedNodeValueCommand(url);

						sqlCommand.Connection = sqlConnection;
						sqlCommand.Transaction = sqlTransaction;
						sqlCommand.ExecuteNonQueryWithRetry(_retryPolicy);
						sqlTransaction.Commit();
					}
				}

				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					sqlConnection.Close();
				}
			}
		}

		public async void CheckAndAssignNextJob(IHttpSender httpSender)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicyTimeout);

					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						var checkAndAssignNextJobCommand = CreateCheckAndAssignNextJobCommand();
						checkAndAssignNextJobCommand.Connection = sqlConnection;
						checkAndAssignNextJobCommand.Transaction = sqlTransaction;

						string nodeUrl;
						JobToDo job;

						using (var sqlDataReader =
							checkAndAssignNextJobCommand.ExecuteReaderWithRetry(_retryPolicyTimeout))
						{
							if (sqlDataReader.HasRows)
							{
								sqlDataReader.Read();

								nodeUrl = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("nodeUrl"));

								job = new JobToDo
								{
									Id = (Guid) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Id")),
									Name = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Name")),
									Serialized = ((string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Serialized"))).Replace(@"\", @""),
									Type = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Type")),
									CreatedBy = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("UserName"))
								};

								sqlDataReader.Close();
							}
							else
							{
								sqlDataReader.Close();
								sqlConnection.Close();

								return;
							}
						}

						var builderHelper = new NodeUriBuilderHelper(nodeUrl);
						var urijob = builderHelper.GetJobTemplateUri();
						var response = await httpSender.PostAsync(urijob, job);

						UpdateDb(response, sqlConnection, sqlTransaction, job, nodeUrl);

						sqlTransaction.Commit();
					}
					sqlConnection.Close();
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
		}

		public async void CancelThisJob(Guid jobId, IHttpSender httpSender)
		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicyTimeout);

					using (var sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.Serializable))
					{
						//--------------------------------------------
						// Select from job definitions.
						//--------------------------------------------
						var selectFromJobDefinitionsWithTabLockCommand =
							CreateSelectFromJobDefinitionsWithTabLockCommand(jobId);

						selectFromJobDefinitionsWithTabLockCommand.Connection = sqlConnection;
						selectFromJobDefinitionsWithTabLockCommand.Transaction = sqlTransaction;

						using (var sqlDataReader =
							selectFromJobDefinitionsWithTabLockCommand.ExecuteReaderWithRetry(_retryPolicyTimeout))
						{
							if (sqlDataReader.HasRows)
							{
								sqlDataReader.Read();
								var node = GetValue(sqlDataReader["AssignedNode"]);
								sqlDataReader.Close();

								if (string.IsNullOrEmpty(node))
								{
									//--------------------------------------------
									// Delete job definition.
									//--------------------------------------------
									var deleteFromJobDefinitionsCommand =
										CreateDeleteFromJobDefinitionsCommand(jobId);

									deleteFromJobDefinitionsCommand.Connection = sqlConnection;
									deleteFromJobDefinitionsCommand.Transaction = sqlTransaction;
									deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);

									//--------------------------------------------
									// Update job history.
									//--------------------------------------------
									var updateCommand = sqlConnection.CreateCommand();
									updateCommand.CommandText =
										"UPDATE [Stardust].JobHistory " +
										"SET Result = @Result " +
										"WHERE JobId = @Id";

									updateCommand.Parameters.AddWithValue("@Id", jobId);
									updateCommand.Parameters.AddWithValue("@Result", "Deleted");
									updateCommand.Transaction = sqlTransaction;
									updateCommand.ExecuteNonQueryWithRetry(_retryPolicy);
								}
								else
								{
									var builderHelper = new NodeUriBuilderHelper(node);
									var uriCancel = builderHelper.GetCancelJobUri(jobId);

									var httpResponseMessage = await httpSender.DeleteAsync(uriCancel);

									if (httpResponseMessage != null && httpResponseMessage.IsSuccessStatusCode)
									{
										//---------------------------------------------------
										// Update job definitions.
										//---------------------------------------------------
										var updateCommand = sqlConnection.CreateCommand();
										updateCommand.CommandText =
											"UPDATE [Stardust].JobDefinitions " +
											"SET Status = 'Canceling' " +
											"WHERE Id = @Id";

										updateCommand.Parameters.AddWithValue("@Id", jobId);
										updateCommand.Transaction = sqlTransaction;
										updateCommand.ExecuteNonQueryWithRetry(_retryPolicy);

										//---------------------------------------------------
										// Insert into job history detail.
										//---------------------------------------------------
										var insertIntoJobHistoryDetailCommand =
											CreateInsertIntoJobHistoryDetailCommand(jobId, "Canceling", DateTime.UtcNow);

										insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
										insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;
										insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicy);
									}
								}
							}
							else
							{
								this.Log().WarningWithLineNumber("[MANAGER, " + Environment.MachineName +
								                                 "] : Could not find job defintion for id : " + jobId);
							}
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

		public void SetEndResultOnJob(Guid jobId, string result)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				try
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var sqlTransaction = sqlConnection.BeginTransaction())
					{
						//-------------------------------------------------------------
						// Set end result.
						//-------------------------------------------------------------
						var setEndResultOnJobCommand =
							CreateSetEndResultOnJobCommand(jobId, result, DateTime.UtcNow);

						setEndResultOnJobCommand.Connection = sqlConnection;
						setEndResultOnJobCommand.Transaction = sqlTransaction;
						setEndResultOnJobCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						//-------------------------------------------------------------
						// Insert into job history detail.
						//-------------------------------------------------------------
						var insertIntoJobHistoryDetailCommand =
							CreateInsertIntoJobHistoryDetailCommand(jobId, result, DateTime.UtcNow);

						insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
						insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;

						insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						sqlTransaction.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					sqlConnection.Close();
				}
			}
		}

		public void ReportProgress(Guid jobId,
		                           string detail,
		                           DateTime created)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				try
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					using (var sqlTransaction = sqlConnection.BeginTransaction())
					{
						//------------------------------------------
						// Insert into job history detail.
						//------------------------------------------
						var insertIntoJobHistoryDetailCommand =
							CreateInsertIntoJobHistoryDetailCommand(jobId, detail, created);

						insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
						insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;
						insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						sqlTransaction.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					sqlConnection.Close();
				}
			}
		}


		public JobHistory History(Guid jobId)

		{
			try
			{
				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var selectJobHistoryByJobIdCommand =
						CreateSelectJobHistoryByJobIdCommand(jobId);

					selectJobHistoryByJobIdCommand.Connection = sqlConnection;

					using (var sqlDataReader =
						selectJobHistoryByJobIdCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							sqlDataReader.Read();
							var jobHist = NewJobHistoryModel(sqlDataReader);
							return jobHist;
						}

						sqlDataReader.Close();
						sqlConnection.Close();
					}
					return null;
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
			return null;
		}

		public IList<JobHistory> HistoryList()
		{
			try
			{
				var returnList = new List<JobHistory>();

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var selectCommand = CreateSelectAllJobHistoryCommand();
					selectCommand.Connection = sqlConnection;

					using (var sqlDataReader = selectCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							while (sqlDataReader.Read())
							{
								var jobHist = NewJobHistoryModel(sqlDataReader);
								returnList.Add(jobHist);
							}
						}
						sqlDataReader.Close();
						sqlConnection.Close();
					}
					return returnList;
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
			return null;
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			try
			{
				var returnList = new List<JobHistoryDetail>();

				using (var sqlConnection = new SqlConnection(_connectionString))
				{
					sqlConnection.OpenWithRetry(_retryPolicy);

					var jobHistoryDetailsByIdCommand = CreateJobHistoryDetailsByIdCommand(jobId);
					jobHistoryDetailsByIdCommand.Connection = sqlConnection;

					using (var sqlDataReader =
						jobHistoryDetailsByIdCommand.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (sqlDataReader.HasRows)
						{
							while (sqlDataReader.Read())
							{
								var detail = new JobHistoryDetail
								{
									Created = (DateTime) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Created")),
									Detail = (string) sqlDataReader.GetValue(sqlDataReader.GetOrdinal("Detail"))
								};
								returnList.Add(detail);
							}
						}
						sqlDataReader.Close();
						sqlConnection.Close();
					}
					return returnList;
				}
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return null;
		}

		private SqlCommand CreateUpdateJobDefinitionsCommand(Guid id,
		                                                     string assignedNode,
		                                                     string status)
		{
			var commandText = "UPDATE [Stardust].JobDefinitions " +
			                  "SET AssignedNode = @AssignedNode, " +
			                  "Status = 'Started' " +
			                  "WHERE Id = @Id";

			var sqlCommand = new SqlCommand(commandText);

			sqlCommand.Parameters.AddWithValue("@Id", id);
			sqlCommand.Parameters.AddWithValue("@AssignedNode", assignedNode);
			sqlCommand.Parameters.AddWithValue("@Status", status);

			return sqlCommand;
		}

		private SqlCommand CreateUpdateJobDefinitionsClearAssignedNodeValueCommand(string assignedNode)
		{
			var commandText = "UPDATE [Stardust].JobDefinitions " +
			                  "SET AssignedNode = null " +
			                  "WHERE AssignedNode = @AssingedNode";

			var sqlCommand = new SqlCommand(commandText);

			sqlCommand.Parameters.AddWithValue("@AssingedNode", assignedNode);

			return sqlCommand;
		}

		private SqlCommand CreateUpdateJobDefinitionsClearAssignedNodeValue(string assignedNode)
		{
			var commandText = "UPDATE [Stardust].JobDefinitions " +
			                  "SET AssignedNode = null " +
			                  "WHERE AssignedNode = @assingedNode";

			var sqlCommand = new SqlCommand(commandText);

			sqlCommand.Parameters.AddWithValue("@assingedNode", assignedNode);

			return sqlCommand;
		}


		private SqlCommand CreateJobHistoryDetailsByIdCommand(Guid id)
		{
			var commmandText = @"SELECT  Created, Detail  
								FROM [Stardust].JobHistoryDetail  
								WHERE JobId = @JobId";

			var sqlCommand = new SqlCommand(commmandText);

			sqlCommand.Parameters.AddWithValue("@JobId", id);

			return sqlCommand;
		}

		private SqlCommand CreateSelectAllFromJobDefinitionsCommand()
		{
			const string selectCommand = @"SELECT  Id    
                                            ,Name
                                            ,Serialized
                                            ,Type
                                            ,UserName
                                            ,AssignedNode
                                             ,Status
                                        FROM [Stardust].JobDefinitions";

			return new SqlCommand(selectCommand);
		}

		private SqlCommand CreateDeleteFromJobDefinitionsCommand(Guid id)
		{
			const string commandText =
				"DELETE FROM[Stardust].JobDefinitions " +
				"WHERE Id = @ID";

			var command = new SqlCommand(commandText);

			command.Parameters.AddWithValue("@ID", id);

			return command;
		}

		private SqlCommand CreateSelectFromJobDefinitionsWithTabLockCommand(Guid id)
		{
			const string commandText =
				"SELECT * From [Stardust].JobDefinitions WITH (TABLOCKX) " +
				"WHERE Id = @Id";

			var command = new SqlCommand(commandText);

			command.Parameters.AddWithValue("@Id", id);

			return command;
		}

		private SqlCommand CreateSetEndResultOnJobCommand(Guid id,
		                                                  string result,
		                                                  DateTime endeDateTime)
		{
			const string commandText =
				"UPDATE [Stardust].JobHistory " +
				"SET Result = @Result, " +
				"Ended = @Ended " +
				"WHERE JobId = @Id";

			var command = new SqlCommand(commandText);

			command.Parameters.AddWithValue("@Id", id);
			command.Parameters.AddWithValue("@Result", result);
			command.Parameters.AddWithValue("@Ended", endeDateTime);

			return command;
		}

		private SqlCommand CreateCheckAndAssignNextJobCommand()
		{
			const string commandText =
				"SELECT Top 1 j.*, n.Url AS nodeUrl FROM [Stardust].JobDefinitions j WITH (TABLOCKX), [Stardust].WorkerNodes n " +
				"WHERE j.AssignedNode IS NULL AND n.Alive = 1 AND n.Url NOT IN (SELECT ISNULL(AssignedNode,'') FROM [Stardust].JobDefinitions)";

			var command = new SqlCommand(commandText);

			return command;
		}

		private SqlCommand CreateInsertIntoJobHistoryCommand(JobDefinition job)
		{
			const string commandText =
				"INSERT INTO [Stardust].JobHistory " +
				"(JobId, Name, CreatedBy, Serialized, Type) " +
				"VALUES(@Id, @Name, @By, @Serialized, @Type)";

			var command = new SqlCommand(commandText);

			command.Parameters.AddWithValue("@Id", job.Id);

			command.Parameters.AddWithValue("@Name", job.Name);

			command.Parameters.AddWithValue("@By", job.UserName);

			command.Parameters.AddWithValue("@Serialized", job.Serialized);

			command.Parameters.AddWithValue("@Type", job.Type);

			return command;
		}

		private SqlCommand CreateInsertIntoJobDefinitionsCommand(JobDefinition jobDefinition)
		{
			const string commandText =
				"INSERT INTO [Stardust].JobDefinitions " +
				"(Id, Name, Serialized, Type, UserName) " +
				"VALUES(@Id, @Name, @Serialized, @Type, @UserName)";

			var command = new SqlCommand(commandText);

			command.Parameters.AddWithValue("@Id", jobDefinition.Id);
			command.Parameters.AddWithValue("@Name", jobDefinition.Name);
			command.Parameters.AddWithValue("@UserName", jobDefinition.UserName);
			command.Parameters.AddWithValue("@Serialized", jobDefinition.Serialized);
			command.Parameters.AddWithValue("@Type", jobDefinition.Type);

			return command;
		}

		private void UpdateDb(HttpResponseMessage response,
		                      SqlConnection sqlConnection,
		                      SqlTransaction sqlTransaction,
		                      JobToDo jobToDo,
		                      string nodeUrl)
		{
			if (response.IsSuccessStatusCode)
			{
				//-------------------------------------------------------------
				//Update job definitions.
				//-------------------------------------------------------------
				var definitionsUpdateCommand =
					CreateUpdateJobDefinitionsCommand(jobToDo.Id, nodeUrl, "Started");

				definitionsUpdateCommand.Connection = sqlConnection;
				definitionsUpdateCommand.Transaction = sqlTransaction;
				definitionsUpdateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//-------------------------------------------------------------
				//Update job history.
				//-------------------------------------------------------------
				var historyUpdateCommand = sqlConnection.CreateCommand();
				historyUpdateCommand.CommandText =
					"UPDATE [Stardust].JobHistory " +
					"SET Started = @Started, " +
					"SentTo = @Node " +
					"WHERE JobId = @Id";

				historyUpdateCommand.Transaction = sqlTransaction;
				historyUpdateCommand.Parameters.AddWithValue("@Id", jobToDo.Id);
				historyUpdateCommand.Parameters.AddWithValue("@Started", DateTime.UtcNow);
				historyUpdateCommand.Parameters.AddWithValue("@Node", nodeUrl);
				historyUpdateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//-------------------------------------------------------------
				// Insert into job history detail.
				//-------------------------------------------------------------
				var insertIntoJobHistoryDetailCommand =
					CreateInsertIntoJobHistoryDetailCommand(jobToDo.Id, "Started", DateTime.UtcNow);

				insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
				insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;
			}

			if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
			{
				//-------------------------------------------------------------
				//Delete job definition.
				//-------------------------------------------------------------
				var deleteFromJobDefinitionsCommand =
					CreateDeleteFromJobDefinitionsCommand(jobToDo.Id);

				deleteFromJobDefinitionsCommand.Connection = sqlConnection;
				deleteFromJobDefinitionsCommand.Transaction = sqlTransaction;
				deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//-------------------------------------------------------------
				//Update job history
				//-------------------------------------------------------------
				var updateCommand = sqlConnection.CreateCommand();
				updateCommand.CommandText = "UPDATE [Stardust].JobHistory " +
				                            "SET Result = @Result, " +
				                            "SentTo = @Node " +
				                            "WHERE JobId = @Id";

				updateCommand.Transaction = sqlTransaction;
				updateCommand.Parameters.AddWithValue("@Id", jobToDo.Id);
				updateCommand.Parameters.AddWithValue("@Result", "Removed because of bad request.");
				updateCommand.Parameters.AddWithValue("@Node", nodeUrl);
				updateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//-------------------------------------------------------------
				//Insert into history detail.
				//-------------------------------------------------------------
				if (response.ReasonPhrase != null)
				{
					var insertIntoJobHistoryDetailCommand =
						CreateInsertIntoJobHistoryDetailCommand(jobToDo.Id, response.ReasonPhrase, DateTime.UtcNow);

					insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
					insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;

					insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
				}
			}
		}

		private SqlCommand CreateInsertIntoJobHistoryDetailCommand(Guid id,
		                                                           string detail,
		                                                           DateTime createDateTime)
		{
			const string commandText =
				"INSERT INTO [Stardust].JobHistoryDetail " +
				"(JobId, Detail, Created) " +
				"VALUES (@Id, @Detail, @Created)";

			var command = new SqlCommand(commandText);

			command.Parameters.AddWithValue("@Id", id);
			command.Parameters.AddWithValue("@Detail", detail);
			command.Parameters.AddWithValue("@Created", createDateTime);

			return command;
		}

		private string GetValue(object value)
		{
			return value == DBNull.Value
				? null
				: (string) value;
		}

		private JobHistory NewJobHistoryModel(SqlDataReader reader)
		{
			try
			{
				var jobHistory = new JobHistory
				{
					Id = (Guid) reader.GetValue(reader.GetOrdinal("JobId")),
					Name = (string) reader.GetValue(reader.GetOrdinal("Name")),
					CreatedBy = (string) reader.GetValue(reader.GetOrdinal("CreatedBy")),
					SentTo = GetValue(reader.GetValue(reader.GetOrdinal("SentTo"))),
					Result = GetValue(reader.GetValue(reader.GetOrdinal("Result"))),
					Created = (DateTime) reader.GetValue(reader.GetOrdinal("Created")),
					Started = GetDateTime(reader.GetValue(reader.GetOrdinal("Started"))),
					Ended = GetDateTime(reader.GetValue(reader.GetOrdinal("Ended")))
				};

				return jobHistory;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
			return null;
		}

		private static SqlCommand CreateSelectJobHistoryByJobIdCommand(Guid id)
		{
			var commandText = @"SELECT 
                                             JobId    
                                            ,Name
                                            ,CreatedBy
                                            ,Created
                                            ,Started
                                            ,Ended
                                            ,SentTo
											,Result
                                        FROM [Stardust].JobHistory WHERE JobId = @JobId";

			var sqlCommand = new SqlCommand(commandText);

			sqlCommand.Parameters.AddWithValue("@JobId", id);

			return sqlCommand;
		}

		private static SqlCommand CreateSelectAllJobHistoryCommand()
		{
			var commandText = @"SELECT 
                                             JobId    
                                            ,Name
                                            ,CreatedBy
                                            ,Created
                                            ,Started
                                            ,Ended
                                            ,SentTo
											,Result
                                        FROM [Stardust].JobHistory";

			return new SqlCommand(commandText);
		}

		private DateTime? GetDateTime(object databaseValue)
		{
			if (databaseValue.Equals(DBNull.Value))
				return null;

			return (DateTime) databaseValue;
		}
	}
}