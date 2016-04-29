using System;
using System.Data.SqlClient;
using Stardust.Manager.Models;

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

		public static SqlCommand CreateInsertIntoJobDetailCommand(Guid jobId, string detail, DateTime created, SqlConnection sqlConnection)
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

		public static SqlCommand CreateGetJobQueueItemByJobIdCommand(Guid jobId, SqlConnection sqlConnection)
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

		public static SqlCommand CreateSelectJobDetailByJobIdCommand(Guid jobId, SqlConnection sqlConnection)
		{
			var selectCommandText = @"SELECT  
											Id, 
											JobId, 
											Created, 
											Detail  
										FROM [Stardust].[JobDetail] WITH (NOLOCK) 
										WHERE JobId = @JobId";

			var selectSqlCommand = new SqlCommand(selectCommandText, sqlConnection);
			selectSqlCommand.Parameters.AddWithValue("@JobId", jobId);

			return selectSqlCommand;
		}



		public static SqlCommand CreateSelectJobThatDidNotEndCommand(string sentToWorkerNodeUri, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
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
										  WHERE  SentToWorkerNodeUri = @SentToWorkerNodeUri AND
												 Started IS NOT NULL AND 
												 Ended IS NULL";

			var selectCommand = new SqlCommand(selectJobByJobIdCommandText, sqlConnection);
			selectCommand.Transaction = sqlTransaction;
			selectCommand.Parameters.AddWithValue("@SentToWorkerNodeUri", sentToWorkerNodeUri);

			return selectCommand;
		}

		public static SqlCommand CreateInsertIntoJobQueueCommand(JobQueueItem jobQueueItem, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			const string insertIntoJobQueueCommandText = @"INSERT INTO [Stardust].[JobQueue]
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

			var insertCommand = new SqlCommand(insertIntoJobQueueCommandText, sqlConnection);
			insertCommand.Transaction = sqlTransaction;

			insertCommand.Parameters.AddWithValue("@JobId", jobQueueItem.JobId);
			insertCommand.Parameters.AddWithValue("@Name", jobQueueItem.Name);
			insertCommand.Parameters.AddWithValue("@Type", jobQueueItem.Type);
			insertCommand.Parameters.AddWithValue("@Serialized", jobQueueItem.Serialized);
			insertCommand.Parameters.AddWithValue("@Created", DateTime.UtcNow);
			insertCommand.Parameters.AddWithValue("@CreatedBy", jobQueueItem.CreatedBy);

			return insertCommand;
		}

		public static SqlCommand CreateDeleteJobByJobIdCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			var deleteJobByIdSqlCommandText = @"DELETE FROM [Stardust].[Job] 
												   WHERE JobId = @JobId";

			var deleteCommand = new SqlCommand(deleteJobByIdSqlCommandText, sqlConnection);
			deleteCommand.Transaction = sqlTransaction;

			deleteCommand.Parameters.AddWithValue("@JobId",jobId);

			return deleteCommand;
		}

		public static SqlCommand CreateSelectAllAliveWorkerNodesCommand(SqlConnection sqlConnection)
		{
			const string selectAllAliveWorkerNodesCommandText = @"SELECT   
												   [Id]
												  ,[Url]
												  ,[Heartbeat]
												  ,[Alive]
											  FROM [Stardust].[WorkerNode] WITH (NOLOCK) 
											  WHERE Alive = 1";

			var sqlCommand = new SqlCommand(selectAllAliveWorkerNodesCommandText, sqlConnection);
			return sqlCommand;
		}

		public static SqlCommand CreateSelectWorkerNodeCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			var selectWorkerNodeUriFromJobCommandText = "SELECT SentToWorkerNodeUri " +
																	"FROM [Stardust].[Job] " +
																	"WHERE JobId = @JobId";

			var selectCommand = new SqlCommand(selectWorkerNodeUriFromJobCommandText, sqlConnection);
			selectCommand.Transaction = sqlTransaction;

			selectCommand.Parameters.AddWithValue("@JobId", jobId);
			return selectCommand;
		}


		public static SqlCommand CreateDoesJobDetailItemExistsCommand(Guid jobId,
														   SqlConnection sqlConnection)
		{
			const string selectCommandText = @"SELECT COUNT(*) 
											FROM [Stardust].[JobDetail]
											WHERE JobId = @JobId";

			var command = new SqlCommand(selectCommandText, sqlConnection);

			command.Parameters.AddWithValue("@JobId", jobId);

			return command;
		}

		public static SqlCommand CreateDoesJobQueueItemExistsCommand(Guid jobId, SqlConnection sqlConnection)
		{
			const string selectCommand = @"SELECT COUNT(*) 
											FROM [Stardust].[JobQueue]
											WHERE JobId = @JobId";

			var sqlCommand = new SqlCommand(selectCommand, sqlConnection);

			sqlCommand.Parameters.AddWithValue("@JobId", jobId);

			return sqlCommand;
		}

		public static SqlCommand CreateDoesJobItemExistsCommand(Guid jobId, SqlConnection sqlConnection)
		{
			const string selectCommand = @"SELECT COUNT(*) 
											FROM [Stardust].[Job]
											WHERE JobId = @JobId";

			var sqlCommand = new SqlCommand(selectCommand, sqlConnection);

			sqlCommand.Parameters.AddWithValue("@JobId", jobId);

			return sqlCommand;
		}
	}
}
