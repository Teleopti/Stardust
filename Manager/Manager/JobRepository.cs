using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using log4net;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Stardust.Manager
{
	public class JobRepository : IJobRepository
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (JobRepository));
		private const int DelaysMiliseconds = 100;
		private const int MaxRetry = 3;
		private readonly string _connectionString;

		public JobRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy> MakeRetryPolicy()
		{
			var fromMilliseconds = TimeSpan.FromMilliseconds(DelaysMiliseconds);
			var policy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(MaxRetry, fromMilliseconds);
			policy.Retrying += (sender, args) =>
			{
				// Log details of the retry.
				var msg = String.Format("Retry - Count:{0}, Delay:{1}, Exception:{2}", args.CurrentRetryCount, args.Delay, args.LastException);
				LogHelper.LogErrorWithLineNumber(Logger, msg);
			};
			return policy;
		}

		private void TryAdd(JobDefinition job)
		{
			var jdDataSet = new DataSet();
			var jdDataTable = new DataTable("[Stardust].[JobDefinitions]");
			jdDataTable.Columns.Add(new DataColumn("Id",
																typeof(Guid)));
			jdDataTable.Columns.Add(new DataColumn("Name",
																typeof(string)));
			jdDataTable.Columns.Add(new DataColumn("Serialized",
																typeof(string)));
			jdDataTable.Columns.Add(new DataColumn("Type",
																typeof(string)));
			jdDataTable.Columns.Add(new DataColumn("AssignedNode",
																typeof(string)));
			jdDataTable.Columns.Add(new DataColumn("UserName",
																typeof(string)));
			jdDataTable.Columns.Add(new DataColumn("Status",
																typeof(string)));
			jdDataSet.Tables.Add(jdDataTable);
			var dr = jdDataTable.NewRow();
			dr["Id"] = job.Id;
			dr["Name"] = job.Name;
			dr["Serialized"] = job.Serialized;
			dr["Type"] = job.Type;
			dr["UserName"] = job.UserName;
			dr["AssignedNode"] = DBNull.Value;
			dr["Status"] = DBNull.Value;
			jdDataTable.Rows.Add(dr);

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				using (var da = new SqlDataAdapter("Select * From [Stardust].JobDefinitions",
					connection))
				{
					var builder = new SqlCommandBuilder(da);
					builder.GetInsertCommand();
					da.Update(jdDataSet,
						"[Stardust].[JobDefinitions]");

					//add a row in history
					da.InsertCommand =
						new SqlCommand(
							"INSERT INTO [Stardust].JobHistory (JobId, Name, CreatedBy, Serialized, Type) VALUES(@Id, @Name, @By, @Serialized, @Type)",
							connection);

					da.InsertCommand.Parameters.Add("@Id",
						SqlDbType.UniqueIdentifier,
						16,
						"JobId");

					da.InsertCommand.Parameters[0].Value = job.Id;

					da.InsertCommand.Parameters.Add("@Name",
						SqlDbType.NVarChar,
						2000,
						"Name");
					da.InsertCommand.Parameters[1].Value = job.Name;
					da.InsertCommand.Parameters.Add("@By",
						SqlDbType.NVarChar,
						500,
						"CreatedBy");

					da.InsertCommand.Parameters[2].Value = job.UserName;

					da.InsertCommand.Parameters.Add("@Serialized",
						SqlDbType.NVarChar,
						2000,
						"Serialized");

					da.InsertCommand.Parameters[3].Value = job.Serialized;

					da.InsertCommand.Parameters.Add("@Type",
						SqlDbType.NVarChar,
						2000,
						"Type");

					da.InsertCommand.Parameters[4].Value = job.Type;

					da.InsertCommand.ExecuteNonQuery();
				}

				ReportProgress(job.Id,
					"Added");

				connection.Close();
			}
		}

		public void Add(JobDefinition job)
		{
			var policy = MakeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => TryAdd(job));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger,ex.Message +  "Unable to add job in database");
			}
		}

		public List<JobDefinition> LoadAll()
		{
			var result = new List<JobDefinition>();
			var policy = MakeRetryPolicy();
			try
			{
				result =  policy.ExecuteAction(()=> TryLoadAll());
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to load jobs from database");
			}
			return result;
		}

		public List<JobDefinition> TryLoadAll()
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			const string selectCommand = @"SELECT 
                                             Id    
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
					connection.Open();

					var command = new SqlCommand
					{
						Connection = connection,
						CommandText = selectCommand,
						CommandType = CommandType.Text
					};

					var reader = command.ExecuteReader();

					if (reader.HasRows)
					{
						while (reader.Read())
						{
							
							var jobDefinition = new JobDefinition
							{
								Id = (Guid) reader.GetValue(reader.GetOrdinal("Id")),
								Name = (string) reader.GetValue(reader.GetOrdinal("Name")),
								Serialized = (string) reader.GetValue(reader.GetOrdinal("Serialized")),
								Type = (string) reader.GetValue(reader.GetOrdinal("Type")),
								UserName = (string) reader.GetValue(reader.GetOrdinal("UserName")),
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
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finshed.");

			return null;
		}

		public void DeleteJob(Guid jobId)
		{
			var policy = MakeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => TrydeleteJob(jobId));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to delete the job");
			}
			
		}

		public void TrydeleteJob(Guid jobId)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (var da = new SqlDataAdapter("Select * From [Stardust].JobDefinitions",
					                                   connection))
					{
						using (var command = new SqlCommand("DELETE FROM [Stardust].JobDefinitions WHERE Id = @ID",
						                                    connection))
						{
							var parameter = command.Parameters.Add("@ID",
							                                       SqlDbType.UniqueIdentifier,
							                                       16,
							                                       "Id");

							parameter.Value = jobId;

							da.DeleteCommand = command;
							da.DeleteCommand.ExecuteNonQuery();
						}
					}

					connection.Close();
				}
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");
		}

		public void FreeJobIfNodeIsAssigned(string url)
		{
			var policy = MakeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => TryFreeJobIfNodeIsAssigned(url));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
		}

		public void TryFreeJobIfNodeIsAssigned(string url)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (var da = new SqlDataAdapter("Select * From [Stardust].JobDefinitions",
					                                   connection))
					{
						using (var command =
							new SqlCommand("Update [Stardust].JobDefinitions Set AssignedNode = null where AssignedNode = @assingedNode",
							               connection))
						{
							var parameter = command.Parameters.Add("@assingedNode",
							                                       SqlDbType.NVarChar,
							                                       2000,
							                                       "AssignedNode");
							parameter.Value = url;
							da.UpdateCommand = command;

							da.UpdateCommand.ExecuteNonQuery();
						}
					}

					connection.Close();
				}
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");
		}

		//verify THAT !!!!!!!!!!!
		public async void TryCheckAndAssignNextJob(List<WorkerNode> availableNodes,
		                                        IHttpSender httpSender)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start CheckAndAssignNextJob.");

			if (!availableNodes.Any())
			{
				LogHelper.LogDebugWithLineNumber(Logger, "Could not find any availabe nodes. Procedure will return.");

				return;
			}

			try
			{
				LogHelper.LogDebugWithLineNumber(Logger,
				                                 "Found ( " + availableNodes.Count + " ) availabe nodes.");
				
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (var tran = connection.BeginTransaction(IsolationLevel.Serializable))
					{
						using (
							var da =
								new SqlDataAdapter(
									"SELECT TOP 1 * From [Stardust].JobDefinitions WITH (TABLOCKX) WHERE AssignedNode IS NULL OR AssignedNode = ''",
									connection)

								{
									SelectCommand =
									{
										Transaction = tran,
										CommandTimeout = 2
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
									Id = (Guid) jobRow["Id"],
									Name = GetValue<string>(jobRow["Name"]),
									Serialized = GetValue<string>(jobRow["Serialized"])
										.Replace(@"\",
										         @""),
									Type = GetValue<string>(jobRow["Type"]),
									CreatedBy = GetValue<string>(jobRow["UserName"])
								};

								da.UpdateCommand =
									new SqlCommand(
										"UPDATE [Stardust].JobDefinitions SET AssignedNode = @AssignedNode, Status = 'Started' WHERE Id = @Id",
										connection);

								var nodeParam = da.UpdateCommand.Parameters.Add("@AssignedNode",
								                                                SqlDbType.NVarChar);
								nodeParam.SourceColumn = "AssignedNode";

								var parameter = da.UpdateCommand.Parameters.Add("@Id",
								                                                SqlDbType.UniqueIdentifier);
								parameter.SourceColumn = "Id";
								parameter.Value = job.Id;

								da.UpdateCommand.Transaction = tran;

								foreach (var node in availableNodes)
								{
										LogHelper.LogDebugWithLineNumber(Logger,
																		 "Available node : ( id, Url ) : ( " + node.Id + ", " + node.Url +
																		 " )");
									
									try
									{
										var builderHelper =
											new NodeUriBuilderHelper(node.Url);

										var urijob = builderHelper.GetJobTemplateUri();

										LogHelper.LogDebugWithLineNumber(Logger,
										                                 "Post async Uri : ( " + urijob + " )");

										var response = await httpSender.PostAsync(urijob,
										                                          job);

										if (response.IsSuccessStatusCode)
										{
											//save
											nodeParam.Value = node.Url.ToString();
											da.UpdateCommand.ExecuteNonQuery();

											//update history
											da.UpdateCommand =
												new SqlCommand("UPDATE [Stardust].JobHistory SET Started = @Started, SentTo = @Node WHERE JobId = @Id",
												               connection);
											da.UpdateCommand.Parameters.Add("@Id",
											                                SqlDbType.UniqueIdentifier,
											                                16,
											                                "JobId");
											da.UpdateCommand.Parameters[0].Value = job.Id;
											da.UpdateCommand.Parameters.Add("@Started",
											                                SqlDbType.DateTime,
											                                16,
											                                "Started");
											da.UpdateCommand.Parameters[1].Value = DateTime.UtcNow;
											da.UpdateCommand.Parameters.Add("@Node",
											                                SqlDbType.NVarChar,
											                                2000,
											                                "SentTo");
											da.UpdateCommand.Parameters[2].Value = node.Url.ToString();

											da.UpdateCommand.Transaction = tran;
											da.UpdateCommand.ExecuteNonQuery();
											ReportProgress(job.Id,
											               "Started");
											break;
										}


										if (response.StatusCode.Equals(HttpStatusCode.BadRequest))
										{
											//remove the job if badrequest
											da.DeleteCommand = new SqlCommand("DELETE FROM [Stardust].JobDefinitions WHERE Id = @Id",
											                                  connection);

											var deleteParameter = da.DeleteCommand.Parameters.Add("@Id",
											                                                      SqlDbType.UniqueIdentifier);
											deleteParameter.SourceColumn = "Id";
											deleteParameter.Value = job.Id;

											da.DeleteCommand.Transaction = tran;
											da.DeleteCommand.ExecuteNonQuery();
											//update history
											da.UpdateCommand =
												new SqlCommand("UPDATE [Stardust].JobHistory SET Result = @Result, SentTo = @Node WHERE JobId = @Id",
												               connection);
											da.UpdateCommand.Parameters.Add("@Id",
											                                SqlDbType.UniqueIdentifier,
											                                16,
											                                "JobId");
											da.UpdateCommand.Parameters[0].Value = job.Id;
											da.UpdateCommand.Parameters.Add("@Result",
											                                SqlDbType.NVarChar,
											                                200,
											                                "Result");
											da.UpdateCommand.Parameters[1].Value = "Removed because of bad request";
											da.UpdateCommand.Parameters.Add("@Node",
											                                SqlDbType.NVarChar,
											                                2000,
											                                "SentTo");
											da.UpdateCommand.Parameters[2].Value = node.Url.ToString();

											da.UpdateCommand.Transaction = tran;
											da.UpdateCommand.ExecuteNonQuery();
										}
									}

									catch (Exception exp)
									{
										LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
									}
								}
							}
						}

						tran.Commit();

						connection.Close();
					}
				}
			}

			catch (SqlException exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}
		}

		public void CheckAndAssignNextJob(List<WorkerNode> availableNodes,
			IHttpSender httpSender)
		{
			var policy = MakeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => TryCheckAndAssignNextJob(availableNodes, httpSender));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
		}

		public async void TryCancelThisJob(Guid jobId,
		                                IHttpSender httpSender)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

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

								da.UpdateCommand.Parameters.Add("@Id",
								                                SqlDbType.UniqueIdentifier,
								                                16,
								                                "JobId");

								da.UpdateCommand.Parameters[0].Value = jobId;

								da.UpdateCommand.Parameters.Add("@Result",
								                                SqlDbType.NVarChar,
								                                2000,
								                                "Result");

								da.UpdateCommand.Parameters[1].Value = "Deleted";

								da.UpdateCommand.Transaction = tran;
								da.UpdateCommand.ExecuteNonQuery();
							}
							else
							{
								var builderHelper = new NodeUriBuilderHelper(node);

								var uriCancel = builderHelper.GetCancelJobUri(jobId);

								LogHelper.LogDebugWithLineNumber(Logger,
								                                 "Send delete async : " + uriCancel);

								var response =
									await httpSender.DeleteAsync(uriCancel);

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
									               "Canceling");
								}
							}
						}
						else
						{
							LogHelper.LogWarningWithLineNumber(Logger,
							                                   "[MANAGER, " + Environment.MachineName +
							                                   "] : Could not find job defintion for id : " + jobId);
						}
					}

					tran.Commit();

					connection.Close();
				}
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");
		}

		public void CancelThisJob(Guid jobId,
			IHttpSender httpSender)
		{
			var policy = MakeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => TryCancelThisJob(jobId, httpSender));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
		}

		public void SetEndResultOnJob(Guid jobId,
												string result)
		{
			var policy = MakeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => TrySetEndResultOnJob(jobId, result));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
		}

		public void TrySetEndResultOnJob(Guid jobId,
		                              string result)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (var da = new SqlDataAdapter("SELECT * From [Stardust].JobHistory",
					                                   connection))
					{
						da.UpdateCommand =
							new SqlCommand("UPDATE [Stardust].JobHistory SET Result = @Result, Ended = @Ended WHERE JobId = @Id",
							               connection);

						da.UpdateCommand.Parameters.Add("@Id",
						                                SqlDbType.UniqueIdentifier,
						                                16,
						                                "JobId");
						da.UpdateCommand.Parameters[0].Value = jobId;
						da.UpdateCommand.Parameters.Add("@Ended",
						                                SqlDbType.DateTime,
						                                16,
						                                "Ended");
						da.UpdateCommand.Parameters[1].Value = DateTime.UtcNow;
						da.UpdateCommand.Parameters.Add("@Result",
						                                SqlDbType.NVarChar,
						                                2000,
						                                "Result");
						da.UpdateCommand.Parameters[2].Value = result;
						da.UpdateCommand.ExecuteNonQuery();
					}

					connection.Close();

					ReportProgress(jobId,
					               result);
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");
		}

		public void ReportProgress(Guid jobId,
											string detail)
		{
			var policy = MakeRetryPolicy();
			try
			{
				policy.ExecuteAction(() => TryReportProgress(jobId, detail));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
		}

		public void TryReportProgress(Guid jobId,
		                           string detail)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (var da = new SqlDataAdapter("SELECT * From [Stardust].JobHistoryDetail",
					                                   connection)
					{
						InsertCommand =
							new SqlCommand("INSERT INTO [Stardust].JobHistoryDetail (JobId, Detail) VALUES (@Id, @Detail)",
							               connection)
					})
					{
						da.InsertCommand.Parameters.Add("@Id",
						                                SqlDbType.UniqueIdentifier,
						                                16,
						                                "JobId");
						da.InsertCommand.Parameters[0].Value = jobId;
						da.InsertCommand.Parameters.Add("@Detail",
						                                SqlDbType.NVarChar,
						                                2000,
						                                "Detail");
						da.InsertCommand.Parameters[1].Value = detail;
						da.InsertCommand.ExecuteNonQuery();
					}

					connection.Close();
				}
			}

			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");
		}

		public JobHistory History(Guid jobId)
		{
			JobHistory jobHist = null;
			var policy = MakeRetryPolicy();
			try
			{
				jobHist =	policy.ExecuteAction(() => TryHistory(jobId));
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
			return jobHist;
		}

		public JobHistory TryHistory(Guid jobId)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

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

					command.Parameters.Add("@JobId",
					                       SqlDbType.UniqueIdentifier,
					                       16,
					                       "JobId");

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
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");

			return null;
		}

		public IList<JobHistory> HistoryList()
		{
			var returnList = new List<JobHistory>();
			var policy = MakeRetryPolicy();
			try
			{
				returnList = policy.ExecuteAction(() => TryHistoryList()).ToList();
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
			return returnList;
		}

		public IList<JobHistory> TryHistoryList()
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

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
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");

			return null;
		}

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			var returnList = new List<JobHistoryDetail>();
			var policy = MakeRetryPolicy();
			try
			{
				returnList = policy.ExecuteAction(() => TryJobHistoryDetails(jobId)).ToList();
			}
			catch (Exception ex)
			{
				LogHelper.LogErrorWithLineNumber(Logger, ex.Message + "Unable to perform operation");
			}
			return returnList;
		}

		public IList<JobHistoryDetail> TryJobHistoryDetails(Guid jobId)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

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
									Created = (DateTime) reader.GetValue(reader.GetOrdinal("Created")),
									Detail = (string) reader.GetValue(reader.GetOrdinal("Detail"))
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
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");

			return null;
		}

		private string GetValue<T>(object value)
		{
			return value == DBNull.Value
				? null
				: (string) value;
		}

		private JobHistory NewJobHistoryModel(SqlDataReader reader)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			try
			{
				var jobHist = new JobHistory
				{
					Id = (Guid) reader.GetValue(reader.GetOrdinal("JobId")),
					Name = (string) reader.GetValue(reader.GetOrdinal("Name")),
					CreatedBy = (string) reader.GetValue(reader.GetOrdinal("CreatedBy")),
					SentTo = GetValue<string>(reader.GetValue(reader.GetOrdinal("SentTo"))),
					Result = GetValue<string>(reader.GetValue(reader.GetOrdinal("Result"))),
					Created = (DateTime) reader.GetValue(reader.GetOrdinal("Created")),
					Started = GetDateTime(reader.GetValue(reader.GetOrdinal("Started"))),
					Ended = GetDateTime(reader.GetValue(reader.GetOrdinal("Ended")))
				};

				return jobHist;
			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");

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
			{
				return null;
			}

			return (DateTime) databaseValue;
		}
	}
}