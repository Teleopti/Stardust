﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
		private readonly RetryPolicyProvider _retryPolicyProvider;
		private readonly RetryPolicy _retryPolicy;
		private readonly string _connectionString;

		public JobRepository(string connectionString, RetryPolicyProvider retryPolicyProvider)
		{
			_connectionString = connectionString;
			_retryPolicyProvider = retryPolicyProvider;
			_retryPolicy = retryPolicyProvider.GetPolicy();
		}

		private void runner(Action funcToRun, string faliureMessage)
		{
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				policy.ExecuteAction(funcToRun);
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + faliureMessage);
			}
		}

		private void applyLoggingOnRetries(RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> policy)
		{
			policy.Retrying += (sender, args) =>
			{
				var msg = String.Format("Retry - Count:{0}, Delay:{1}, Exception:{2}", args.CurrentRetryCount, args.Delay,
					args.LastException);
				this.Log().ErrorWithLineNumber(msg);
			};
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
						tran.Commit();
						ReportProgress(job.Id, "Added", DateTime.Now);
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

		//public void CheckAndAssignNextJob(List<WorkerNode> availableNodes, IHttpSender httpSender)
		//{
		//	runner(() => TryCheckAndAssignNextJob(availableNodes, httpSender), "Unable to perform operation");
		//}

		public async void CheckAndAssignNextJob(List<WorkerNode> availableNodes,
												IHttpSender httpSender)
		{
			if (!availableNodes.Any()) return;

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);
					using (var tran = connection.BeginTransaction(IsolationLevel.Serializable))
					{
						using (
							var da =
								new SqlDataAdapter(
									"SELECT TOP 1 * FROM [Stardust].JobDefinitions WITH (TABLOCKX) WHERE AssignedNode IS NULL OR AssignedNode = ''",
									connection)
								{
									SelectCommand =
									{
										Transaction = tran,
										CommandTimeout = 10
									}
								})
						{
							var jobs = new DataTable();
							da.Fill(jobs);

							if (jobs.Rows.Count > 0)
							{
								var jobRow = jobs.Rows[0];
								var job = new JobToDo
								{
									Id = (Guid)jobRow["Id"],
									Name = GetValue<string>(jobRow["Name"]),
									Serialized = GetValue<string>(jobRow["Serialized"]).Replace(@"\", @""),
									Type = GetValue<string>(jobRow["Type"]),
									CreatedBy = GetValue<string>(jobRow["UserName"])
								};

								da.UpdateCommand =
									new SqlCommand(
										"UPDATE [Stardust].JobDefinitions SET AssignedNode = @AssignedNode, Status = 'Started' WHERE Id = @Id",
										connection);

								var nodeParam = da.UpdateCommand.Parameters.Add("@AssignedNode", SqlDbType.NVarChar);
								nodeParam.SourceColumn = "AssignedNode";

								var parameter = da.UpdateCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
								parameter.SourceColumn = "Id";
								parameter.Value = job.Id;

								da.UpdateCommand.Transaction = tran;

								foreach (var node in availableNodes)
								{
									try
									{
										var builderHelper = new NodeUriBuilderHelper(node.Url);
										var urijob = builderHelper.GetJobTemplateUri();
										HttpResponseMessage response = await httpSender.PostAsync(urijob, job);

										if (response.IsSuccessStatusCode)
										{
											//save
											nodeParam.Value = node.Url.ToString();
											da.UpdateCommand.ExecuteNonQueryWithRetry(_retryPolicy);

											//update history
											da.UpdateCommand =
												new SqlCommand("UPDATE [Stardust].JobHistory SET Started = @Started, SentTo = @Node WHERE JobId = @Id",
												               connection);

											da.UpdateCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier, 16, "JobId");
											da.UpdateCommand.Parameters[0].Value = job.Id;

											da.UpdateCommand.Parameters.Add("@Started", SqlDbType.DateTime, 16, "Started");
											da.UpdateCommand.Parameters[1].Value = DateTime.UtcNow;

											da.UpdateCommand.Parameters.Add("@Node", SqlDbType.NVarChar, 2000, "SentTo");
											da.UpdateCommand.Parameters[2].Value = node.Url.ToString();

											da.UpdateCommand.Transaction = tran;
											da.UpdateCommand.ExecuteNonQueryWithRetry(_retryPolicy);

											ReportProgress(job.Id, "Started", DateTime.Now);

											break;
										}
										if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
										{
											//remove the job if badrequest
											da.DeleteCommand = new SqlCommand("DELETE FROM [Stardust].JobDefinitions WHERE Id = @Id",
											                                  connection);
											var deleteParameter = da.DeleteCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
											deleteParameter.SourceColumn = "Id";
											deleteParameter.Value = job.Id;

											da.DeleteCommand.Transaction = tran;
											da.DeleteCommand.ExecuteNonQueryWithRetry(_retryPolicy);

											//update history
											da.UpdateCommand =
												new SqlCommand("UPDATE [Stardust].JobHistory " +
												               "SET Result = @Result, SentTo = @Node WHERE JobId = @Id",
												               connection);

											da.UpdateCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier, 16, "JobId");
											da.UpdateCommand.Parameters[0].Value = job.Id;

											da.UpdateCommand.Parameters.Add("@Result", SqlDbType.NVarChar, 200, "Result");
											da.UpdateCommand.Parameters[1].Value = "Removed because of bad request";

											da.UpdateCommand.Parameters.Add("@Node", SqlDbType.NVarChar, 2000, "SentTo");
											da.UpdateCommand.Parameters[2].Value = node.Url.ToString();

											da.UpdateCommand.Transaction = tran;
											da.UpdateCommand.ExecuteNonQueryWithRetry(_retryPolicy);

											//insert into history detail.
											if (response.ReasonPhrase != null)
											{
												string insertcommand = @"INSERT INTO [Stardust].[JobHistoryDetail]
																	([JobId]
																	,[Created]
																	,[Detail])
																VALUES
																	(@JobId
																	,@Created
																	,@Detail)";

												da.InsertCommand = new SqlCommand(insertcommand, connection);

												da.InsertCommand.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier, 16);
												da.InsertCommand.Parameters[0].Value = job.Id;


												da.InsertCommand.Parameters.Add("@Detail",
												                                SqlDbType.NText);
												da.InsertCommand.Parameters[1].Value = response.ReasonPhrase;

												da.InsertCommand.Parameters.Add("@Created", SqlDbType.DateTime, 16);
												da.InsertCommand.Parameters[2].Value = DateTime.Now;

												da.InsertCommand.Transaction = tran;
												da.InsertCommand.ExecuteNonQueryWithRetry(_retryPolicy);
											}

										}
									}
									catch (TimeoutException exp)
									{
										this.Log().InfoWithLineNumber("Timeout: " + exp.Message);
									}
									catch (Exception exp)
									{
										this.Log().ErrorWithLineNumber(exp.Message, exp);
									}
								}
							}
						}
						tran.Commit();
						connection.Close();
					}
				}
			}
			catch (TimeoutException exp)
			{
				this.Log().InfoWithLineNumber("Timeout: " + exp.Message);
			}
			catch (SqlException exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}


		public void CancelThisJob(Guid jobId, IHttpSender httpSender)
		{
			runner(() => tryCancelThisJob(jobId, httpSender), "Unable to  cancel the job");
		}

		private async void tryCancelThisJob(Guid jobId, IHttpSender httpSender)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();
					var tran = connection.BeginTransaction(IsolationLevel.Serializable);

					using (var da =
						new SqlDataAdapter(string.Format("SELECT * From [Stardust].JobDefinitions  WITH (TABLOCKX) WHERE Id = '{0}'",
														 jobId),
										   connection)
						{
							SelectCommand =
							{
								Transaction = tran
							}
						})
					{
						var jobs = new DataTable();
						da.Fill(jobs);
						if (jobs.Rows.Count > 0)
						{
							var jobRow = jobs.Rows[0];
							var node = GetValue<string>(jobRow["AssignedNode"]);

							if (string.IsNullOrEmpty(node))
							{
								da.DeleteCommand = new SqlCommand("DELETE FROM [Stardust].JobDefinitions WHERE Id = @Id",
																  connection);

								var parameter = da.DeleteCommand.Parameters.Add("@Id",
																				SqlDbType.UniqueIdentifier);
								parameter.SourceColumn = "Id";
								parameter.Value = jobId;
								da.DeleteCommand.Transaction = tran;
								da.DeleteCommand.ExecuteNonQuery();

								//update history
								da.UpdateCommand = new SqlCommand("UPDATE [Stardust].JobHistory SET Result = @Result WHERE JobId = @Id",
																  connection);

								da.UpdateCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier, 16, "JobId");
								da.UpdateCommand.Parameters[0].Value = jobId;

								da.UpdateCommand.Parameters.Add("@Result", SqlDbType.NVarChar, 2000, "Result");
								da.UpdateCommand.Parameters[1].Value = "Deleted";

								da.UpdateCommand.Transaction = tran;
								da.UpdateCommand.ExecuteNonQuery();
							}
							else
							{
								var builderHelper = new NodeUriBuilderHelper(node);
								var uriCancel = builderHelper.GetCancelJobUri(jobId);

								var response = await httpSender.DeleteAsync(uriCancel);

								if (response != null && response.IsSuccessStatusCode)
								{
									da.UpdateCommand = new SqlCommand("UPDATE [Stardust].JobDefinitions SET Status = 'Canceling' WHERE Id = @Id",
																	  connection);

									var parameter = da.UpdateCommand.Parameters.Add("@Id",
																					SqlDbType.UniqueIdentifier);
									parameter.SourceColumn = "Id";
									parameter.Value = jobId;

									da.UpdateCommand.Transaction = tran;
									da.UpdateCommand.ExecuteNonQuery();

									ReportProgress(jobId,
												   "Canceling",
												   DateTime.Now);
								}
							}
						}
						else
						{
							this.Log().WarningWithLineNumber("[MANAGER, " + Environment.MachineName +
															   "] : Could not find job defintion for id : " + jobId);
						}
					}
					tran.Commit();
					connection.Close();
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}

		}

		public void SetEndResultOnJob(Guid jobId, string result)
		{
			runner(() => trySetEndResultOnJob(jobId, result), "Unable to set end result on the job");
		}

		private void trySetEndResultOnJob(Guid jobId, string result)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (var da = new SqlDataAdapter("SELECT * From [Stardust].JobHistory", connection))
					{
						da.UpdateCommand =
							new SqlCommand("UPDATE [Stardust].JobHistory SET Result = @Result, Ended = @Ended WHERE JobId = @Id",
										   connection);

						da.UpdateCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier, 16, "JobId");
						da.UpdateCommand.Parameters[0].Value = jobId;

						da.UpdateCommand.Parameters.Add("@Ended", SqlDbType.DateTime, 16, "Ended");
						da.UpdateCommand.Parameters[1].Value = DateTime.UtcNow;

						da.UpdateCommand.Parameters.Add("@Result", SqlDbType.NVarChar, 2000, "Result");
						da.UpdateCommand.Parameters[2].Value = result;

						da.UpdateCommand.ExecuteNonQuery();
					}

					connection.Close();
					ReportProgress(jobId, result, DateTime.Now);
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}

		public void ReportProgress(Guid jobId, string detail, DateTime created)
		{
			runner(() => tryReportProgress(jobId, detail, created), "Unable to report progress on job");
		}

		private void tryReportProgress(Guid jobId, string detail, DateTime created)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (var da = new SqlDataAdapter("SELECT * From [Stardust].JobHistoryDetail",
													   connection)
					{
						InsertCommand =
							new SqlCommand(
								"INSERT INTO [Stardust].JobHistoryDetail (JobId, Detail, Created) VALUES (@Id, @Detail, @Created)",
								connection)
					})
					{
						da.InsertCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier, 16, "JobId");
						da.InsertCommand.Parameters[0].Value = jobId;

						da.InsertCommand.Parameters.Add("@Detail", SqlDbType.NText);
						da.InsertCommand.Parameters[1].Value = detail;

						da.InsertCommand.Parameters.Add("@Created", SqlDbType.DateTime);
						da.InsertCommand.Parameters[2].Value = created;

						da.InsertCommand.ExecuteNonQuery();
					}
					connection.Close();
				}
			}
			catch (Exception exp)
			{
				this.Log().ErrorWithLineNumber(exp.Message, exp);
			}
		}


		public JobHistory History(Guid jobId)
		{
			JobHistory jobHist = null;
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				jobHist = policy.ExecuteAction(() => tryHistory(jobId));
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + "Unable to perform operation");
			}
			return jobHist;
		}

		private JobHistory tryHistory(Guid jobId)

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

					command.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier, 16, "JobId");
					command.Parameters[0].Value = jobId;

					connection.Open();
					using (var reader = command.ExecuteReader())
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
			var returnList = new List<JobHistory>();
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				returnList = policy.ExecuteAction(() => tryHistoryList()).ToList();
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + "Unable to perform operation");
			}
			return returnList;
		}

		private IList<JobHistory> tryHistoryList()

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

					connection.Open();
					using (var reader = command.ExecuteReader())
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
			var returnList = new List<JobHistoryDetail>();
			var policy = _retryPolicyProvider.GetPolicy();
			applyLoggingOnRetries(policy);
			try
			{
				returnList = policy.ExecuteAction(() => tryJobHistoryDetails(jobId)).ToList();
			}
			catch (Exception ex)
			{
				this.Log().ErrorWithLineNumber(ex.Message + "Unable to perform operation");
			}
			return returnList;
		}

		private IList<JobHistoryDetail> tryJobHistoryDetails(Guid jobId)
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
					command.Parameters.Add("@JobId", SqlDbType.UniqueIdentifier, 16, "JobId");
					command.Parameters[0].Value = jobId;

					connection.Open();

					using (var reader = command.ExecuteReader())
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