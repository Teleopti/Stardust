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

		public void Add(JobDefinition job)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				try
				{
					connection.OpenWithRetry(_retryPolicy);

					using (var sqlTransaction = connection.BeginTransaction())
					{
						// Insert into job definitions.
						var insertIntoJobDefinitionsCommand = CreateInsertIntoJobDefinitionsCommand();
						insertIntoJobDefinitionsCommand.Connection = connection;
						insertIntoJobDefinitionsCommand.Transaction = sqlTransaction;

						insertIntoJobDefinitionsCommand.Parameters["@Id"].Value = job.Id;
						insertIntoJobDefinitionsCommand.Parameters["@Name"].Value = job.Name;
						insertIntoJobDefinitionsCommand.Parameters["@UserName"].Value = job.UserName;
						insertIntoJobDefinitionsCommand.Parameters["@Serialized"].Value = job.Serialized;
						insertIntoJobDefinitionsCommand.Parameters["@Type"].Value = job.Type;

						// Insert into job history.
						var insertIntoJobHistoryCommand = CreateInsertIntoJobHistoryCommand();
						insertIntoJobHistoryCommand.Connection = connection;
						insertIntoJobHistoryCommand.Transaction = sqlTransaction;

						insertIntoJobHistoryCommand.Parameters["@Id"].Value = job.Id;
						insertIntoJobHistoryCommand.Parameters["@Name"].Value = job.Name;
						insertIntoJobHistoryCommand.Parameters["@By"].Value = job.UserName;
						insertIntoJobHistoryCommand.Parameters["@Serialized"].Value = job.Serialized;
						insertIntoJobHistoryCommand.Parameters["@Type"].Value = job.Type;

						//Insert into job history detail. 
						var insertIntoJobHistoryDetailCommand = CreateInsertIntoJobHistoryDetailCommand();
						insertIntoJobHistoryDetailCommand.Connection = connection;
						insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;

						insertIntoJobHistoryDetailCommand.Parameters["@Id"].Value = job.Id;
						insertIntoJobHistoryDetailCommand.Parameters["@Detail"].Value = "Added";
						insertIntoJobHistoryDetailCommand.Parameters["@Created"].Value = DateTime.UtcNow;

						insertIntoJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);
						insertIntoJobHistoryCommand.ExecuteNonQueryWithRetry(_retryPolicy);
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
					connection.Close();
				}
			}
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

		public List<JobDefinition> LoadAll()
		{
			try
			{
				var listToReturn = new List<JobDefinition>();

				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);

					var selectAllFromJobDefinitionsCommand = CreateSelectAllFromJobDefinitionsCommand();
					selectAllFromJobDefinitionsCommand.Connection = connection;

					var reader =
						selectAllFromJobDefinitionsCommand.ExecuteReaderWithRetry(_retryPolicy);

					if (reader.HasRows)
					{
						while (reader.Read())
						{
							var jobDefinition = new JobDefinition
							{
								Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
								Name = (string)reader.GetValue(reader.GetOrdinal("Name")),
								Serialized = (string)reader.GetValue(reader.GetOrdinal("Serialized")),
								Type = (string)reader.GetValue(reader.GetOrdinal("Type")),
								UserName = (string)reader.GetValue(reader.GetOrdinal("UserName")),
								AssignedNode = GetValue<string>(reader.GetValue(reader.GetOrdinal("AssignedNode"))),
								Status = GetValue<string>(reader.GetValue(reader.GetOrdinal("Status")))
							};

							listToReturn.Add(jobDefinition);
						}
					}
					reader.Close();
					connection.Close();
				}
				return listToReturn;
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			return null;
		}

		private SqlCommand CreateDeleteFromJobDefinitionsCommand()
		{
			const string commandText =
				"DELETE FROM[Stardust].JobDefinitions WHERE Id = @ID";

			SqlCommand command = new SqlCommand(commandText);

			command.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);

			return command;
		}

		public void DeleteJob(Guid jobId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				try
				{
					connection.OpenWithRetry(_retryPolicy);

					using (var tran = connection.BeginTransaction())
					{
						var deleteFromJobDefinitionsCommand = CreateDeleteFromJobDefinitionsCommand();
						deleteFromJobDefinitionsCommand.Connection = connection;
						deleteFromJobDefinitionsCommand.Parameters["@ID"].Value = jobId;

						deleteFromJobDefinitionsCommand.Transaction = tran;
						deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						tran.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					connection.Close();
				}
			}
		}

		public void FreeJobIfNodeIsAssigned(string url)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				try
				{
					connection.OpenWithRetry(_retryPolicy);

					using (var tran = connection.BeginTransaction())
					{
						var deleteCommand = connection.CreateCommand();
						deleteCommand.CommandText =
							"Update [Stardust].JobDefinitions " +
							"Set AssignedNode = null where AssignedNode = @assingedNode";

						deleteCommand.Parameters.AddWithValue("@assingedNode", url);

						deleteCommand.Transaction = tran;
						deleteCommand.ExecuteNonQueryWithRetry(_retryPolicy);
						tran.Commit();
					}
				}

				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					connection.Close();
				}
			}
		}

		public async void CheckAndAssignNextJob(IHttpSender httpSender)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicyTimeout);

					using (var tran = connection.BeginTransaction(IsolationLevel.Serializable))
					{
						var checkAndAssignNextJobCommand = CreateCheckAndAssignNextJobCommand();
						checkAndAssignNextJobCommand.Connection = connection;
						checkAndAssignNextJobCommand.Transaction = tran;

						string nodeUrl;
						JobToDo job;

						using (var reader = checkAndAssignNextJobCommand.ExecuteReaderWithRetry(_retryPolicyTimeout))
						{
							if (reader.HasRows)
							{
								reader.Read();

								nodeUrl = (string)reader.GetValue(reader.GetOrdinal("nodeUrl"));

								job = new JobToDo
								{
									Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
									Name = (string)reader.GetValue(reader.GetOrdinal("Name")),
									Serialized = ((string)reader.GetValue(reader.GetOrdinal("Serialized"))).Replace(@"\", @""),
									Type = (string)reader.GetValue(reader.GetOrdinal("Type")),
									CreatedBy = (string)reader.GetValue(reader.GetOrdinal("UserName"))
								};

								reader.Close();
							}
							else
							{
								reader.Close();
								connection.Close();

								return;
							}
						}

						var builderHelper = new NodeUriBuilderHelper(nodeUrl);
						var urijob = builderHelper.GetJobTemplateUri();
						var response = await httpSender.PostAsync(urijob, job);

						UpdateDb(response, connection, tran, job, nodeUrl);

						tran.Commit();
					}
					connection.Close();
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

		private SqlCommand CreateSelectFromJobDefinitionsWithTabLockCommand()
		{
			const string commandText =
				"SELECT * From [Stardust].JobDefinitions WITH (TABLOCKX) WHERE Id = @Id";

			SqlCommand command = new SqlCommand(commandText);

			command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);

			return command;
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
						var selectFromJobDefinitionsWithTabLockCommand = CreateSelectFromJobDefinitionsWithTabLockCommand();
						selectFromJobDefinitionsWithTabLockCommand.Connection = sqlConnection;
						selectFromJobDefinitionsWithTabLockCommand.Transaction = sqlTransaction;

						selectFromJobDefinitionsWithTabLockCommand.Parameters["@Id"].Value = jobId;

						using (var reader = selectFromJobDefinitionsWithTabLockCommand.ExecuteReaderWithRetry(_retryPolicyTimeout))
						{
							if (reader.HasRows)
							{
								reader.Read();
								var node = GetValue<string>(reader["AssignedNode"]);
								reader.Close();

								if (string.IsNullOrEmpty(node))
								{
									var deleteFromJobDefinitionsCommand = CreateDeleteFromJobDefinitionsCommand();

									deleteFromJobDefinitionsCommand.Connection = sqlConnection;
									deleteFromJobDefinitionsCommand.Transaction = sqlTransaction;

									deleteFromJobDefinitionsCommand.Parameters["@Id"].Value = jobId;
									deleteFromJobDefinitionsCommand.ExecuteNonQueryWithRetry(_retryPolicy);

									var updateCommand = sqlConnection.CreateCommand();
									updateCommand.CommandText = "UPDATE [Stardust].JobHistory SET Result = @Result WHERE JobId = @Id";
									updateCommand.Parameters.AddWithValue("@Id", jobId);
									updateCommand.Parameters.AddWithValue("@Result", "Deleted");
									updateCommand.Transaction = sqlTransaction;
									updateCommand.ExecuteNonQueryWithRetry(_retryPolicy);
								}
								else
								{
									var builderHelper = new NodeUriBuilderHelper(node);
									var uriCancel = builderHelper.GetCancelJobUri(jobId);

									var response = await httpSender.DeleteAsync(uriCancel);

									if (response != null && response.IsSuccessStatusCode)
									{
										var updateCommand = sqlConnection.CreateCommand();
										updateCommand.CommandText = "UPDATE [Stardust].JobDefinitions SET Status = 'Canceling' WHERE Id = @Id";
										updateCommand.Parameters.AddWithValue("@Id", jobId);
										updateCommand.Transaction = sqlTransaction;
										updateCommand.ExecuteNonQueryWithRetry(_retryPolicy);

										var insertIntoJobHistoryDetailCommand = CreateInsertIntoJobHistoryDetailCommand();

										insertIntoJobHistoryDetailCommand.Connection = sqlConnection;

										insertIntoJobHistoryDetailCommand.Parameters["@Id"].Value = jobId;
										insertIntoJobHistoryDetailCommand.Parameters["@Detail"].Value = "Canceling";
										insertIntoJobHistoryDetailCommand.Parameters["@Created"].Value = DateTime.UtcNow;

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

		private SqlCommand CreateSetEndResultOnJobCommand()
		{
			const string commandText =
				"UPDATE [Stardust].JobHistory SET Result = @Result, Ended = @Ended WHERE JobId = @Id";

			var command = new SqlCommand(commandText);

			command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
			command.Parameters.Add("@Result", SqlDbType.NText);
			command.Parameters.Add("@Ended", SqlDbType.DateTime);

			return command;
		}

		public void SetEndResultOnJob(Guid jobId, string result)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				try
				{
					connection.OpenWithRetry(_retryPolicy);

					using (var tran = connection.BeginTransaction())
					{
						// Set end result.
						var setEndResultOnJobCommand = CreateSetEndResultOnJobCommand();
						setEndResultOnJobCommand.Connection = connection;
						setEndResultOnJobCommand.Transaction = tran;

						setEndResultOnJobCommand.Parameters["@Id"].Value = jobId;
						setEndResultOnJobCommand.Parameters["@Result"].Value = result;
						setEndResultOnJobCommand.Parameters["@Ended"].Value = DateTime.UtcNow;

						setEndResultOnJobCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						// Insert into job history detail.
						var insertIntoJobHistoryDetailCommand = CreateInsertIntoJobHistoryDetailCommand();
						insertIntoJobHistoryDetailCommand.Connection = connection;
						insertIntoJobHistoryDetailCommand.Transaction = tran;

						insertIntoJobHistoryDetailCommand.Parameters["@Id"].Value = jobId;
						insertIntoJobHistoryDetailCommand.Parameters["@Detail"].Value = result;
						insertIntoJobHistoryDetailCommand.Parameters["@Created"].Value = DateTime.UtcNow;

						insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						tran.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					connection.Close();
				}
			}
		}

		public void ReportProgress(Guid jobId,
								   string detail,
								   DateTime created)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				try
				{
					connection.OpenWithRetry(_retryPolicy);

					using (var tran = connection.BeginTransaction())
					{
						var insertIntoJobHistoryDetailCommand = CreateInsertIntoJobHistoryDetailCommand();

						insertIntoJobHistoryDetailCommand.Connection = connection;

						insertIntoJobHistoryDetailCommand.Parameters["@Id"].Value = jobId;
						insertIntoJobHistoryDetailCommand.Parameters["@Detail"].Value = detail;
						insertIntoJobHistoryDetailCommand.Parameters["@Created"].Value = created;

						insertIntoJobHistoryDetailCommand.Transaction = tran;
						insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicy);

						tran.Commit();
					}
				}
				catch (Exception exp)
				{
					this.Log().ErrorWithLineNumber(exp.Message, exp);
				}
				finally
				{
					connection.Close();
				}
			}
		}


		public JobHistory History(Guid jobId)

		{
			try
			{
				var selectCommand = SelectHistoryCommand(true);

				using (var connection = new SqlConnection(_connectionString))
				{
					var command = new SqlCommand
					{
						Connection = connection,
						CommandText = selectCommand,
						CommandType = CommandType.Text
					};

					command.Parameters.AddWithValue("@JobId", jobId);

					connection.OpenWithRetry(_retryPolicy);
					using (var reader = command.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (reader.HasRows)
						{
							reader.Read();
							var jobHist = NewJobHistoryModel(reader);
							return jobHist;
						}
						reader.Close();
						connection.Close();
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
				var selectCommand = SelectHistoryCommand(false);
				var returnList = new List<JobHistory>();
				using (var connection = new SqlConnection(_connectionString))
				{
					var command = new SqlCommand
					{
						Connection = connection,
						CommandText = selectCommand,
						CommandType = CommandType.Text
					};

					connection.OpenWithRetry(_retryPolicy);

					using (var reader = command.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (reader.HasRows)
						{
							while (reader.Read())
							{
								var jobHist = NewJobHistoryModel(reader);
								returnList.Add(jobHist);
							}
						}
						reader.Close();
						connection.Close();
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
				var selectCommand =
					@"SELECT  Created, Detail  FROM [Stardust].JobHistoryDetail WHERE JobId = @JobId";

				var returnList = new List<JobHistoryDetail>();
				using (var connection = new SqlConnection(_connectionString))
				{
					var command = new SqlCommand
					{
						Connection = connection,
						CommandText = selectCommand,
						CommandType = CommandType.Text
					};
					command.Parameters.AddWithValue("@JobId", jobId);

					connection.OpenWithRetry(_retryPolicy);

					using (var reader = command.ExecuteReaderWithRetry(_retryPolicy))
					{
						if (reader.HasRows)
						{
							while (reader.Read())
							{
								var detail = new JobHistoryDetail
								{
									Created = (DateTime)reader.GetValue(reader.GetOrdinal("Created")),
									Detail = (string)reader.GetValue(reader.GetOrdinal("Detail"))
								};
								returnList.Add(detail);
							}
						}
						reader.Close();
						connection.Close();
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

		private SqlCommand CreateCheckAndAssignNextJobCommand()
		{
			const string commandText =
				"SELECT Top 1 j.*, n.Url AS nodeUrl FROM [Stardust].JobDefinitions j WITH (TABLOCKX), [Stardust].WorkerNodes n " +
				"WHERE j.AssignedNode IS NULL AND n.Alive = 1 AND n.Url NOT IN (SELECT ISNULL(AssignedNode,'') FROM [Stardust].JobDefinitions)";

			var command = new SqlCommand(commandText);

			return command;
		}

		private SqlCommand CreateInsertIntoJobHistoryCommand()
		{
			const string commandText =
				"INSERT INTO [Stardust].JobHistory (JobId, Name, CreatedBy, Serialized, Type) " +
				"VALUES(@Id, @Name, @By, @Serialized, @Type)";

			var command = new SqlCommand(commandText);

			command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
			command.Parameters.Add("@Name", SqlDbType.NText);
			command.Parameters.Add("@By", SqlDbType.NText);
			command.Parameters.Add("@Serialized", SqlDbType.NText);
			command.Parameters.Add("@Type", SqlDbType.NText);

			return command;
		}

		private SqlCommand CreateInsertIntoJobDefinitionsCommand()
		{
			const string commandText =
				"INSERT INTO [Stardust].JobDefinitions (Id, Name, Serialized, Type, UserName) " +
				"VALUES(@Id, @Name, @Serialized, @Type, @UserName)";

			var command = new SqlCommand(commandText);

			command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
			command.Parameters.Add("@Name", SqlDbType.NText);
			command.Parameters.Add("@UserName", SqlDbType.NText);
			command.Parameters.Add("@Serialized", SqlDbType.NText);
			command.Parameters.Add("@Type", SqlDbType.NText);

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
				//save
				var definitionsUpdateCommand = sqlConnection.CreateCommand();
				definitionsUpdateCommand.CommandText =
					"UPDATE [Stardust].JobDefinitions SET AssignedNode = @AssignedNode, Status = 'Started' WHERE Id = @Id";
				definitionsUpdateCommand.Transaction = sqlTransaction;
				definitionsUpdateCommand.Parameters.AddWithValue("@Id", jobToDo.Id);
				definitionsUpdateCommand.Parameters.AddWithValue("@AssignedNode", nodeUrl);
				definitionsUpdateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//update history
				var historyUpdateCommand = sqlConnection.CreateCommand();
				historyUpdateCommand.CommandText =
					"UPDATE [Stardust].JobHistory SET Started = @Started, SentTo = @Node WHERE JobId = @Id";
				historyUpdateCommand.Transaction = sqlTransaction;
				historyUpdateCommand.Parameters.AddWithValue("@Id", jobToDo.Id);
				historyUpdateCommand.Parameters.AddWithValue("@Started", DateTime.UtcNow);
				historyUpdateCommand.Parameters.AddWithValue("@Node", nodeUrl);
				historyUpdateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				var insertIntoJobHistoryDetailCommand = CreateInsertIntoJobHistoryDetailCommand();
				insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
				insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;

				insertIntoJobHistoryDetailCommand.Parameters["@Id"].Value = jobToDo.Id;
				insertIntoJobHistoryDetailCommand.Parameters["@Detail"].Value = "Started";
				insertIntoJobHistoryDetailCommand.Parameters["@Created"].Value = DateTime.UtcNow;
			}

			if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
			{
				//remove the job if badrequest
				var deleteCommand = sqlConnection.CreateCommand();
				deleteCommand.CommandText = "DELETE FROM [Stardust].JobDefinitions WHERE Id = @Id";
				deleteCommand.Transaction = sqlTransaction;
				deleteCommand.Parameters.AddWithValue("@Id", jobToDo.Id);
				deleteCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//update history
				var updateCommand = sqlConnection.CreateCommand();
				updateCommand.CommandText = "UPDATE [Stardust].JobHistory " +
											"SET Result = @Result, SentTo = @Node WHERE JobId = @Id";
				updateCommand.Transaction = sqlTransaction;
				updateCommand.Parameters.AddWithValue("@Id", jobToDo.Id);
				updateCommand.Parameters.AddWithValue("@Result", "Removed because of bad request");
				updateCommand.Parameters.AddWithValue("@Node", nodeUrl);
				updateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//insert into history detail.
				if (response.ReasonPhrase != null)
				{
					var insertIntoJobHistoryDetailCommand = CreateInsertIntoJobHistoryDetailCommand();

					insertIntoJobHistoryDetailCommand.Connection = sqlConnection;
					insertIntoJobHistoryDetailCommand.Transaction = sqlTransaction;

					insertIntoJobHistoryDetailCommand.Parameters["@Id"].Value = jobToDo.Id;
					insertIntoJobHistoryDetailCommand.Parameters["@Detail"].Value = "Started";
					insertIntoJobHistoryDetailCommand.Parameters["@Created"].Value = DateTime.UtcNow;

					insertIntoJobHistoryDetailCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
				}
			}
		}

		private SqlCommand CreateInsertIntoJobHistoryDetailCommand()
		{
			const string commandText =
				"INSERT INTO [Stardust].JobHistoryDetail (JobId, Detail, Created) " +
				"VALUES (@Id, @Detail, @Created)";

			var command = new SqlCommand(commandText);

			command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
			command.Parameters.Add("@Detail", SqlDbType.NText);
			command.Parameters.Add("@Created", SqlDbType.DateTime);

			return command;
		}

		private string GetValue<T>(object value)
		{
			return value == DBNull.Value
				? null
				: (string)value;
		}

		private JobHistory NewJobHistoryModel(SqlDataReader reader)
		{
			try
			{
				var jobHist = new JobHistory
				{
					Id = (Guid)reader.GetValue(reader.GetOrdinal("JobId")),
					Name = (string)reader.GetValue(reader.GetOrdinal("Name")),
					CreatedBy = (string)reader.GetValue(reader.GetOrdinal("CreatedBy")),
					SentTo = GetValue<string>(reader.GetValue(reader.GetOrdinal("SentTo"))),
					Result = GetValue<string>(reader.GetValue(reader.GetOrdinal("Result"))),
					Created = (DateTime)reader.GetValue(reader.GetOrdinal("Created")),
					Started = GetDateTime(reader.GetValue(reader.GetOrdinal("Started"))),
					Ended = GetDateTime(reader.GetValue(reader.GetOrdinal("Ended")))
				};

				return jobHist;
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
			return null;
		}

		private static string SelectHistoryCommand(bool addParameter)
		{
			var selectCommand = @"SELECT 
                                             JobId    
                                            ,Name
                                            ,CreatedBy
                                            ,Created
                                            ,Started
                                            ,Ended
                                            ,SentTo,
                                                                                                                                                                                          Result
                                        FROM [Stardust].JobHistory";

			if (addParameter) selectCommand += " WHERE JobId = @JobId";

			return selectCommand;
		}

		private DateTime? GetDateTime(object databaseValue)
		{
			if (databaseValue.Equals(DBNull.Value))
				return null;

			return (DateTime)databaseValue;
		}
	}
}