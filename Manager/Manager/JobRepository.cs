using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using log4net;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
    public class JobRepository : IJobRepository
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (JobRepository));

        private readonly string _connectionString;

        public JobRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(JobDefinition job)
        {
            var jdDataSet = new DataSet();
            var jdDataTable = new DataTable("[Stardust].[JobDefinitions]");
            jdDataTable.Columns.Add(new DataColumn("Id",
                                                   typeof (Guid)));
            jdDataTable.Columns.Add(new DataColumn("Name",
                                                   typeof (string)));
            jdDataTable.Columns.Add(new DataColumn("Serialized",
                                                   typeof (string)));
            jdDataTable.Columns.Add(new DataColumn("Type",
                                                   typeof (string)));
            jdDataTable.Columns.Add(new DataColumn("AssignedNode",
                                                   typeof (string)));
            jdDataTable.Columns.Add(new DataColumn("JobProgress",
                                                   typeof (string)));
            jdDataTable.Columns.Add(new DataColumn("UserName",
                                                   typeof (string)));
            jdDataTable.Columns.Add(new DataColumn("Status",
                                                   typeof (string)));
            jdDataSet.Tables.Add(jdDataTable);
            var dr = jdDataTable.NewRow();
            dr["Id"] = job.Id;
            dr["Name"] = job.Name;
            dr["Serialized"] = job.Serialized;
            dr["Type"] = job.Type;
            dr["UserName"] = job.UserName;
            dr["AssignedNode"] = DBNull.Value;
            dr["JobProgress"] = DBNull.Value;
            dr["Status"] = DBNull.Value;
            jdDataTable.Rows.Add(dr);

            try
            {
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
                        da.InsertCommand = new SqlCommand("INSERT INTO [Stardust].JobHistory (JobId, Name, CreatedBy, Serialized, Type) VALUES(@Id, @Name, @By, @Serialized, @Type)",
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
            catch (TimeoutException exception)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 "Can not add Job because of timeout",
                                                 exception);
            }
            catch (Exception exception)
            {
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 "Can not add Job because of Error",
                                                 exception);
            }
        }

        private string getValue<T>(object value)
        {
            return value == DBNull.Value
                ? null
                : (string) value;
        }

        public List<JobDefinition> LoadAll()
        {
            const string selectCommand = @"SELECT 
                                             Id    
                                            ,Name
                                            ,Serialized
                                            ,Type
                                            ,UserName
                                            ,AssignedNode
                                            ,JobProgress,
															Status
                                        FROM [Stardust].JobDefinitions";

            var listToReturn = new List<JobDefinition>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = selectCommand,
                    CommandType = CommandType.Text
                };
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var a = getValue<string>(reader.GetValue(reader.GetOrdinal("AssignedNode")));
                        var jobDefinition = new JobDefinition
                        {
                            Id = (Guid) reader.GetValue(reader.GetOrdinal("Id")),
                            Name = (string) reader.GetValue(reader.GetOrdinal("Name")),
                            Serialized = (string) reader.GetValue(reader.GetOrdinal("Serialized")),
                            Type = (string) reader.GetValue(reader.GetOrdinal("Type")),
                            UserName = (string) reader.GetValue(reader.GetOrdinal("UserName")),
                            AssignedNode = getValue<string>(reader.GetValue(reader.GetOrdinal("AssignedNode"))),
                            Status = getValue<string>(reader.GetValue(reader.GetOrdinal("Status"))),
                            JobProgress = getValue<string>(reader.GetValue(reader.GetOrdinal("JobProgress")))
                        };
                        listToReturn.Add(jobDefinition);
                    }
                }
                reader.Close();
                connection.Close();
            }
            return listToReturn;
        }


        public void DeleteJob(Guid jobId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var da = new SqlDataAdapter("Select * From [Stardust].JobDefinitions",
                                                   connection))
                {
                    using (var command = new SqlCommand(
                        "DELETE FROM [Stardust].JobDefinitions WHERE Id = @ID",
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

        public void FreeJobIfNodeIsAssigned(string url)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var da = new SqlDataAdapter("Select * From [Stardust].JobDefinitions",
                                                   connection))
                {
                    using (var command = new SqlCommand("Update [Stardust].JobDefinitions Set AssignedNode = null where AssignedNode = @assingedNode",
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

        public async void CheckAndAssignNextJob(List<WorkerNode> availableNodes,
                                                IHttpSender httpSender)
        {
            if (!availableNodes.Any())
            {
                return;
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var tran = connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        using (var da = new SqlDataAdapter("SELECT * From [Stardust].JobDefinitions  WITH (TABLOCKX) WHERE AssignedNode IS NULL OR AssignedNode = ''",
                                                           connection)

                        {
                            SelectCommand = {Transaction = tran, CommandTimeout = 2}
                        })
                        {
                            DataTable jobs = new DataTable();

                            da.Fill(jobs);

                            if (jobs.Rows.Count > 0)
                            {
                                DataRow jobRow = jobs.Rows[0];
                                var job = new JobToDo
                                {
                                    Id = (Guid) jobRow["Id"],
                                    Name = getValue<string>(jobRow["Name"]),
                                    Serialized = getValue<string>(jobRow["Serialized"])
                                        .Replace(@"\",
                                                 @""),
                                    Type = getValue<string>(jobRow["Type"]),
                                    CreatedBy = getValue<string>(jobRow["UserName"])
                                };

                                da.UpdateCommand = new SqlCommand("UPDATE [Stardust].JobDefinitions SET AssignedNode = @AssignedNode, Status = 'Started' WHERE Id = @Id",
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
                                    try
                                    {
                                        NodeUriBuilderHelper builderHelper = new NodeUriBuilderHelper(node.Url);
                                        var urijob = builderHelper.GetJobTemplateUri();

                                        var response = await httpSender.PostAsync(urijob,
                                                                                  job);

                                        if (response != null && response.IsSuccessStatusCode)
                                        {
                                            //save
                                            nodeParam.Value = node.Url.ToString();
                                            da.UpdateCommand.ExecuteNonQuery();

                                            //update history
                                            da.UpdateCommand = new SqlCommand("UPDATE [Stardust].JobHistory SET Started = @Started, SentTo = @Node WHERE JobId = @Id",
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

                                        if (response != null && response.StatusCode.Equals(HttpStatusCode.BadRequest))
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
                                            da.UpdateCommand = new SqlCommand("UPDATE [Stardust].JobHistory SET Result = @Result, SentTo = @Node WHERE JobId = @Id",
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

                                    catch (Exception exception)
                                    {
                                        //log it
                                        var mess = exception.Message;
                                    }
                                }
                            }
                        }

                        tran.Commit();

                        connection.Close();
                    }
                }
            }
            catch (SqlException sqlException)
            {
                //probably timeout
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 "Timeout in Check and assign",
                                                 sqlException.InnerException);
                //System.Threading.Thread.Sleep(5000);
            }
            catch (Exception exception)
            {
                //log it
                LogHelper.LogErrorWithLineNumber(Logger,
                                                 "Error in Check and Assign job",
                                                 exception);
            }
        }

        public async void CancelThisJob(Guid jobId,
                                        IHttpSender httpSender)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var tran = connection.BeginTransaction(IsolationLevel.Serializable);

                using (var da =
                    new SqlDataAdapter(string.Format("SELECT * From [Stardust].JobDefinitions  WITH (TABLOCKX) WHERE Id = '{0}'",
                                                     jobId),
                                       connection)
                    {SelectCommand = {Transaction = tran}})
                {
                    DataTable jobs = new DataTable();

                    da.Fill(jobs);

                    if (jobs.Rows.Count > 0)
                    {
                        DataRow jobRow = jobs.Rows[0];
                        var node = getValue<string>(jobRow["AssignedNode"]);
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
                            NodeUriBuilderHelper builderHelper = new NodeUriBuilderHelper(node);

                            var uriCancel = builderHelper.GetCancelJobUri(jobId);

                            LogHelper.LogInfoWithLineNumber(Logger,
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
                            else
                            {
                                //??? did the node we thought not work on this job, what to do
                            }
                        }
                    }
                    else
                    {
                        LogHelper.LogWarningWithLineNumber(Logger,
                                                           "[MANAGER, " + Environment.MachineName + "] : Could not find job defintion for id : " + jobId);
                    }
                }

                tran.Commit();

                connection.Close();
            }
        }

        public void SetEndResultOnJob(Guid jobId,
                                      string result)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var da = new SqlDataAdapter("SELECT * From [Stardust].JobHistory",
                                                   connection))
                {
                    da.UpdateCommand = new SqlCommand("UPDATE [Stardust].JobHistory SET Result = @Result, Ended = @Ended WHERE JobId = @Id",
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

        public void ReportProgress(Guid jobId,
                                   string detail)
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

        public JobHistory History(Guid jobId)
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

		public IList<JobHistory> HistoryList()
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
						reader.Read();
						var jobHist = NewJobHistoryModel(reader);
						returnList.Add(jobHist);
					}

					reader.Close();
					connection.Close();
				}

				return returnList;
			}
		}

		private JobHistory NewJobHistoryModel(SqlDataReader reader)
	    {
		    var jobHist = new JobHistory
		    {
			    Id = (Guid) reader.GetValue(reader.GetOrdinal("JobId")),
			    Name = (string) reader.GetValue(reader.GetOrdinal("Name")),
			    CreatedBy = (string) reader.GetValue(reader.GetOrdinal("CreatedBy")),
			    SentTo = getValue<string>(reader.GetValue(reader.GetOrdinal("SentTo"))),
			    Result = getValue<string>(reader.GetValue(reader.GetOrdinal("Result"))),
			    Created = (DateTime) (reader.GetValue(reader.GetOrdinal("Created"))),
			    Started = getDateTime(reader.GetValue(reader.GetOrdinal("Started"))),
			    Ended = getDateTime(reader.GetValue(reader.GetOrdinal("Ended")))
		    };
		    return jobHist;
	    }

	    private static string SelectHistoryCommand(bool addParameter)
	    {
		    string selectCommand = @"SELECT 
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

	    private DateTime? getDateTime(object databaseValue)
        {
            if (databaseValue.Equals(DBNull.Value))
            {
                return null;
            }
            return (DateTime) databaseValue;
        }
    }
}