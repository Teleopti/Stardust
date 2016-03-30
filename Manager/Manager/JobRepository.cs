using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
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
		private readonly RetryPolicy _retryPolicy;
		private readonly string _connectionString;
		private RetryPolicy<SqlAzureTransientErrorDetectionStrategyWithTimeouts> _retryPolicyTimeout;

		public JobRepository(string connectionString, RetryPolicyProvider retryPolicyProvider)
		{
			_connectionString = connectionString;
			_retryPolicy = retryPolicyProvider.GetPolicy();
			_retryPolicyTimeout = retryPolicyProvider.GetPolicyWithTimeout();
		}

		public void Add(JobDefinition job)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				SqlCommand jobHistoryCommand = connection.CreateCommand();
				jobHistoryCommand.CommandText = "INSERT INTO [Stardust].JobHistory (JobId, Name, CreatedBy, Serialized, Type) VALUES(@Id, @Name, @By, @Serialized, @Type)";
				jobHistoryCommand.Parameters.AddWithValue("@Id", job.Id);
				jobHistoryCommand.Parameters.AddWithValue("@Name", job.Name);
				jobHistoryCommand.Parameters.AddWithValue("@By", job.UserName);
				jobHistoryCommand.Parameters.AddWithValue("@Serialized", job.Serialized);
				jobHistoryCommand.Parameters.AddWithValue("@Type", job.Type);

				SqlCommand jobDefinitionCommand = connection.CreateCommand();
				jobDefinitionCommand.CommandText = "INSERT INTO [Stardust].JobDefinitions (Id, Name, Serialized, Type, userName) VALUES(@Id, @Name, @Serialized, @Type, @UserName)";
				jobDefinitionCommand.Parameters.AddWithValue("@Id", job.Id);
				jobDefinitionCommand.Parameters.AddWithValue("@Name", job.Name);
				jobDefinitionCommand.Parameters.AddWithValue("@UserName", job.UserName);
				jobDefinitionCommand.Parameters.AddWithValue("@Serialized", job.Serialized);
				jobDefinitionCommand.Parameters.AddWithValue("@Type", job.Type);
				try
				{
					connection.OpenWithRetry(_retryPolicy);
					using (var tran = connection.BeginTransaction())
					{
						jobDefinitionCommand.Transaction = tran;
						jobHistoryCommand.Transaction = tran;
						jobDefinitionCommand.ExecuteNonQueryWithRetry(_retryPolicy);
						jobHistoryCommand.ExecuteNonQueryWithRetry(_retryPolicy);
						ReportProgress(job.Id, "Added", DateTime.UtcNow);
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


		public List<JobDefinition> LoadAll()
		{
			const string selectCommand = @"SELECT  Id    
                                            ,Name
                                            ,Serialized
                                            ,Type
                                            ,UserName
                                            ,AssignedNode
                                             ,Status
                                        FROM [Stardust].JobDefinitions";
			try
			{
				var listToReturn = new List<JobDefinition>();
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);
					var command = new SqlCommand
					{
						Connection = connection,
						CommandText = selectCommand,
						CommandType = CommandType.Text
					};
					var reader = command.ExecuteReaderWithRetry(_retryPolicy);
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

		public void DeleteJob(Guid jobId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				SqlCommand deleteCommand = connection.CreateCommand();
				deleteCommand.CommandText = "DELETE FROM[Stardust].JobDefinitions WHERE Id = @ID";
				deleteCommand.Parameters.AddWithValue("@ID", jobId);

				try
				{
					connection.OpenWithRetry(_retryPolicy);

					using (var tran = connection.BeginTransaction())
					{
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

		public void FreeJobIfNodeIsAssigned(string url)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				SqlCommand deleteCommand = connection.CreateCommand();
				deleteCommand.CommandText = "Update [Stardust].JobDefinitions Set AssignedNode = null where AssignedNode = @assingedNode";
				deleteCommand.Parameters.AddWithValue("@assingedNode", url);

				try
				{
					connection.OpenWithRetry(_retryPolicy);
					using (var tran = connection.BeginTransaction())
					{
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
						var command = connection.CreateCommand();
						command.CommandText = "SELECT Top 1 j.*, n.Url AS nodeUrl FROM [Stardust].JobDefinitions j WITH (TABLOCKX), [Stardust].WorkerNodes n " +
						                      "WHERE j.AssignedNode IS NULL AND n.Alive = 1 AND n.Url NOT IN (SELECT ISNULL(AssignedNode,'') FROM [Stardust].JobDefinitions)";
						command.Transaction = tran;

						string nodeUrl;
						JobToDo job;
						using (var reader = command.ExecuteReaderWithRetry(_retryPolicyTimeout))
						{
							if (reader.HasRows)
							{
								reader.Read();
								nodeUrl = (string) reader.GetValue(reader.GetOrdinal("nodeUrl"));
								job = new JobToDo
								{
									Id = (Guid) reader.GetValue(reader.GetOrdinal("Id")),
									Name = (string) reader.GetValue(reader.GetOrdinal("Name")),
									Serialized = ((string) reader.GetValue(reader.GetOrdinal("Serialized"))).Replace(@"\", @""),
									Type = (string) reader.GetValue(reader.GetOrdinal("Type")),
									CreatedBy = (string) reader.GetValue(reader.GetOrdinal("UserName"))
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

		private void UpdateDb(HttpResponseMessage response, SqlConnection connection, SqlTransaction tran, JobToDo job, string nodeUrl)
		{
			if (response.IsSuccessStatusCode)
			{
				//save
				var definitionsUpdateCommand = connection.CreateCommand();
				definitionsUpdateCommand.CommandText = "UPDATE [Stardust].JobDefinitions SET AssignedNode = @AssignedNode, Status = 'Started' WHERE Id = @Id";
				definitionsUpdateCommand.Transaction = (SqlTransaction) tran;
				definitionsUpdateCommand.Parameters.AddWithValue("@Id", job.Id);
				definitionsUpdateCommand.Parameters.AddWithValue("@AssignedNode", nodeUrl);
				definitionsUpdateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//update history
				var historyUpdateCommand = connection.CreateCommand();
				historyUpdateCommand.CommandText = "UPDATE [Stardust].JobHistory SET Started = @Started, SentTo = @Node WHERE JobId = @Id";
				historyUpdateCommand.Transaction = (SqlTransaction) tran;
				historyUpdateCommand.Parameters.AddWithValue("@Id", job.Id);
				historyUpdateCommand.Parameters.AddWithValue("@Started", DateTime.UtcNow);
				historyUpdateCommand.Parameters.AddWithValue("@Node", nodeUrl);
				historyUpdateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				ReportProgress(job.Id, "Started", DateTime.UtcNow);
			}
			if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
			{
				//remove the job if badrequest
				var deleteCommand = connection.CreateCommand();
				deleteCommand.CommandText = "DELETE FROM [Stardust].JobDefinitions WHERE Id = @Id";
				deleteCommand.Transaction = (SqlTransaction) tran;
				deleteCommand.Parameters.AddWithValue("@Id", job.Id);
				deleteCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//update history
				var updateCommand = connection.CreateCommand();
				updateCommand.CommandText = "UPDATE [Stardust].JobHistory " + "SET Result = @Result, SentTo = @Node WHERE JobId = @Id";
				updateCommand.Transaction = (SqlTransaction) tran;
				updateCommand.Parameters.AddWithValue("@Id", job.Id);
				updateCommand.Parameters.AddWithValue("@Result", "Removed because of bad request");
				updateCommand.Parameters.AddWithValue("@Node", nodeUrl);
				updateCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);

				//insert into history detail.
				if (response.ReasonPhrase != null)
				{
					string insertcommand = @"INSERT INTO [Stardust].[JobHistoryDetail] ([JobId] ,[Created],[Detail]) VALUES (@JobId ,@Created ,@Detail)";
					var insertCommand = connection.CreateCommand();
					insertCommand.CommandText = insertcommand;
					insertCommand.Transaction = (SqlTransaction) tran;
					insertCommand.Parameters.AddWithValue("@JobId", job.Id);
					insertCommand.Parameters.AddWithValue("@Detail", response.ReasonPhrase);
					insertCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
					insertCommand.ExecuteNonQueryWithRetry(_retryPolicyTimeout);
				}
			}
		}

		public async void CancelThisJob(Guid jobId, IHttpSender httpSender)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicyTimeout);
					using (var tran = connection.BeginTransaction(IsolationLevel.Serializable))
					{
						var command = connection.CreateCommand();
						command.CommandText = string.Format("SELECT * From [Stardust].JobDefinitions WITH (TABLOCKX) WHERE Id = '{0}'", jobId);
						command.Transaction = tran;

						using (var reader = command.ExecuteReaderWithRetry(_retryPolicyTimeout))
						{
							if (reader.HasRows)
							{
								reader.Read();
								var node = GetValue<string>(reader["AssignedNode"]);
								reader.Close();

								if (string.IsNullOrEmpty(node))
								{
									var deleteCommand = connection.CreateCommand();
									deleteCommand.CommandText = "DELETE FROM [Stardust].JobDefinitions WHERE Id = @Id";
									deleteCommand.Parameters.AddWithValue("@Id", jobId);
									deleteCommand.Transaction = tran;
									deleteCommand.ExecuteNonQueryWithRetry(_retryPolicy);

									var updateCommand = connection.CreateCommand();
									updateCommand.CommandText = "UPDATE [Stardust].JobHistory SET Result = @Result WHERE JobId = @Id";
									updateCommand.Parameters.AddWithValue("@Id", jobId);
									updateCommand.Parameters.AddWithValue("@Result", "Deleted");
									updateCommand.Transaction = tran;
									updateCommand.ExecuteNonQueryWithRetry(_retryPolicy);
								}
								else
								{
									var builderHelper = new NodeUriBuilderHelper(node);
									var uriCancel = builderHelper.GetCancelJobUri(jobId);

									var response = await httpSender.DeleteAsync(uriCancel);
									if (response != null && response.IsSuccessStatusCode)
									{
										var updateCommand = connection.CreateCommand();
										updateCommand.CommandText = "UPDATE [Stardust].JobDefinitions SET Status = 'Canceling' WHERE Id = @Id";
										updateCommand.Parameters.AddWithValue("@Id", jobId);
										updateCommand.Transaction = tran;
										updateCommand.ExecuteNonQueryWithRetry(_retryPolicy);

										ReportProgress(jobId, "Canceling", DateTime.UtcNow);
									}
								}
							}
							else
							{
								this.Log().WarningWithLineNumber("[MANAGER, " + Environment.MachineName + "] : Could not find job defintion for id : " + jobId);
							}
						}
						tran.Commit();
					}
					connection.Close();
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}

		public
			void SetEndResultOnJob(Guid jobId, string result)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				SqlCommand command = connection.CreateCommand();
				command.CommandText = "UPDATE [Stardust].JobHistory SET Result = @Result, Ended = @Ended WHERE JobId = @Id";
				command.Parameters.AddWithValue("@Id", jobId);
				command.Parameters.AddWithValue("@Result", result);
				command.Parameters.AddWithValue("@Ended", DateTime.UtcNow);

				try
				{
					connection.OpenWithRetry(_retryPolicy);
					using (var tran = connection.BeginTransaction())
					{
						command.Transaction = tran;
						command.ExecuteNonQueryWithRetry(_retryPolicy);
						ReportProgress(jobId, result, DateTime.UtcNow);
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

		public void ReportProgress(Guid jobId, string detail, DateTime created)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				SqlCommand command = connection.CreateCommand();
				command.CommandText = "INSERT INTO [Stardust].JobHistoryDetail (JobId, Detail, Created) VALUES (@Id, @Detail, @Created)";
				command.Parameters.AddWithValue("@Id", jobId);
				command.Parameters.AddWithValue("@Detail", detail);
				command.Parameters.AddWithValue("@Created", created);

				try
				{
					connection.OpenWithRetry(_retryPolicy);
					using (var tran = connection.BeginTransaction())
					{
						command.Transaction = tran;
						command.ExecuteNonQueryWithRetry(_retryPolicy);
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
				var selectCommand = @"SELECT  Created, Detail  FROM [Stardust].JobHistoryDetail WHERE JobId = @JobId";
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
