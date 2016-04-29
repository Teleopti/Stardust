using System;
using System.Data.SqlClient;

namespace Stardust.Manager.Helpers
{
	public static class CreateCommandHelper
	{
		public static SqlCommand CreateUpdateResultCommand(Guid jobId, string result, DateTime ended, SqlConnection sqlConnection)
		{
			var updateJobCommandText = "UPDATE [Stardust].[Job] " +
			                           "SET Result = @Result," +
			                           "Ended = @Ended " +
			                           "WHERE JobId = @JobId";
			var updateCommand = new SqlCommand(updateJobCommandText, sqlConnection);

			updateCommand.Parameters.AddWithValue("@JobId", jobId);
			updateCommand.Parameters.AddWithValue("@Result", result);
			updateCommand.Parameters.AddWithValue("@Ended", ended);
			return updateCommand;
		}

		public static SqlCommand CreateInsertIntoJobDetailSqlCommand(Guid jobId, string detail, DateTime created, SqlConnection sqlConnection)
		{
			const string insertIntoJobDetailCommandText = @"INSERT INTO [Stardust].[JobDetail]
																([JobId]
																,[Created]
																,[Detail])
															VALUES
																(@JobId
																,@Created
																,@Detail)";
			var insertCommand = new SqlCommand(insertIntoJobDetailCommandText, sqlConnection);

			insertCommand.Parameters.AddWithValue("@JobId", jobId);
			insertCommand.Parameters.AddWithValue("@Detail", detail);
			insertCommand.Parameters.AddWithValue("@Created", created);
			return insertCommand;
		}

		public static SqlCommand CreateGetJobQueueItemByJobIdSqlCommand(Guid jobId, SqlConnection sqlConnection)
		{
			const string selectJobQueueItemCommandText =
				@"SELECT  [JobId]
								  ,[Name]
								  ,[Serialized]
								  ,[Type]
								  ,[CreatedBy]
								  ,[Created]
						  FROM [ManagerDb].[Stardust].[JobQueue]
						  WHERE JobId = @JobId";
			var selectSqlCommand = new SqlCommand(selectJobQueueItemCommandText, sqlConnection);

			selectSqlCommand.Parameters.AddWithValue("@JobId", jobId);
			return selectSqlCommand;
		}

		public static SqlCommand CreateSelectJobByJobIdCommand(Guid jobId, SqlConnection sqlConnection)
		{
			const string selectJobByJobIdCommandText = @"SELECT [JobId]
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

			var selectSqlCommand = new SqlCommand(selectJobByJobIdCommandText, sqlConnection);

			selectSqlCommand.Parameters.AddWithValue("@JobId", jobId);
			return selectSqlCommand;
		}

		public static SqlCommand CreateGetAllJobsCommand(SqlConnection sqlConnection)
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
										  FROM [Stardust].[Job] WITH (NOLOCK)";
			var selectSqlCommand = new SqlCommand(selectCommandText, sqlConnection);

			return selectSqlCommand;
		}

		public static SqlCommand CreateGetAllExecutingJobsCommand(SqlConnection sqlConnection)
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
										  FROM [ManagerDB].[Stardust].[Job] WITH (NOLOCK)
										  WHERE [Started] IS NOT NULL AND [Ended] IS NULL";

			var selectSqlCommand = new SqlCommand(selectCommandText, sqlConnection);

			return selectSqlCommand;
		}



	}
}
