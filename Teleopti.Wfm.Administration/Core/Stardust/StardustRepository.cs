using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public class StardustRepository
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(StardustRepository));

		private readonly string _connectionString;

		public StardustRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private string getValue<T>(object value)
		{
			return value == DBNull.Value
				 ? null
				 : (string)value;
		}

		public JobHistory History(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

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

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		public IList<JobHistory> HistoryList()
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

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

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private JobHistory NewJobHistoryModel(SqlDataReader reader)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

			try
			{
				var jobHist = new JobHistory
				{
					Id = (Guid)reader.GetValue(reader.GetOrdinal("JobId")),
					Name = (string)reader.GetValue(reader.GetOrdinal("Name")),
					CreatedBy = (string)reader.GetValue(reader.GetOrdinal("CreatedBy")),
					SentTo = getValue<string>(reader.GetValue(reader.GetOrdinal("SentTo"))),
					Result = getValue<string>(reader.GetValue(reader.GetOrdinal("Result"))),
					Created = (DateTime)(reader.GetValue(reader.GetOrdinal("Created"))),
					Started = getDateTime(reader.GetValue(reader.GetOrdinal("Started"))),
					Ended = getDateTime(reader.GetValue(reader.GetOrdinal("Ended")))
				};

				return jobHist;

			}
			catch (Exception exp)
			{
				LogHelper.LogErrorWithLineNumber(Logger, exp.Message, exp);
			}

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;

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

		public IList<JobHistoryDetail> JobHistoryDetails(Guid jobId)
		{
			LogHelper.LogInfoWithLineNumber(Logger, "Start.");

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
									Created = (DateTime)(reader.GetValue(reader.GetOrdinal("Created"))),
									Detail = (string)(reader.GetValue(reader.GetOrdinal("Detail"))),
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

			LogHelper.LogInfoWithLineNumber(Logger, "Finished.");

			return null;
		}

		private DateTime? getDateTime(object databaseValue)
		{
			if (databaseValue.Equals(DBNull.Value))
			{
				return null;
			}

			return (DateTime)databaseValue;
		}
	}
}