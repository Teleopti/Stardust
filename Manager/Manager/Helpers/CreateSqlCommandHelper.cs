using System;
using System.Data.SqlClient;
using Stardust.Manager.Models;

namespace Stardust.Manager.Helpers
{
	public class CreateSqlCommandHelper
	{
		public SqlCommand CreateUpdateResultCommand(Guid jobId, string result, DateTime ended, SqlConnection sqlConnection)
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

		public SqlCommand CreateInsertIntoJobDetailCommand(Guid jobId, string detail, DateTime created, SqlConnection sqlConnection)
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

		public SqlCommand CreateGetJobQueueItemByJobIdCommand(Guid jobId, SqlConnection sqlConnection)
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

		public SqlCommand CreateSelectJobByJobIdCommand(Guid jobId, SqlConnection sqlConnection)
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


		public SqlCommand CreateSelect1JobQueueItemCommand(SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			var selectOneJobQueueItemCommandText = @"SELECT Top 1  [JobId],
								[Name],
								[Created],
								[Type],
								[Serialized],
								[CreatedBy]
				FROM [Stardust].[JobQueue] 
				ORDER BY Created";

			var selectSqlCommand = new SqlCommand(selectOneJobQueueItemCommandText, sqlConnection);
			selectSqlCommand.Transaction = sqlTransaction;
			
			return selectSqlCommand;
		}

		public SqlCommand CreateUpdateCancellingResultCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			var updateJobSetResultCommandText = "UPDATE [Stardust].[Job] " +
												"SET Result = @Result " +
												"WHERE JobId = @JobId";

			var updateCommand = new SqlCommand(updateJobSetResultCommandText, sqlConnection);
			updateCommand.Transaction = sqlTransaction;
			updateCommand.Parameters.AddWithValue("@JobId", jobId);
			updateCommand.Parameters.AddWithValue("@Result", "Cancelling...");

			return updateCommand;
		}


		public SqlCommand CreateInsertIntoJobCommand(JobQueueItem jobQueueItem, string sentToWorkerNodeUri, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
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

			var insertCommand = new SqlCommand(insertIntoJobCommandText, sqlConnection);
			insertCommand.Transaction = sqlTransaction;

			insertCommand.Parameters.AddWithValue("@JobId", jobQueueItem.JobId);
			insertCommand.Parameters.AddWithValue("@Name", jobQueueItem.Name);
			insertCommand.Parameters.AddWithValue("@Type", jobQueueItem.Type);
			insertCommand.Parameters.AddWithValue("@Serialized", jobQueueItem.Serialized);
			insertCommand.Parameters.AddWithValue("@Started", DateTime.UtcNow);
			insertCommand.Parameters.AddWithValue("@SentToWorkerNodeUri", sentToWorkerNodeUri);
			insertCommand.Parameters.AddWithValue("@CreatedBy", jobQueueItem.CreatedBy);
			insertCommand.Parameters.AddWithValue("@Created", jobQueueItem.Created);
			insertCommand.Parameters.AddWithValue("@Ended", DBNull.Value);

			return insertCommand;
		}


		public SqlCommand CreateGetAllJobsCommand(SqlConnection sqlConnection)
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

		public SqlCommand CreateGetAllExecutingJobsCommand(SqlConnection sqlConnection)
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

		public SqlCommand CreateSelectJobDetailByJobIdCommand(Guid jobId, SqlConnection sqlConnection)
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



		public SqlCommand CreateSelectJobThatDidNotEndCommand(string sentToWorkerNodeUri, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
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

		public SqlCommand CreateInsertIntoJobQueueCommand(JobQueueItem jobQueueItem, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
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

		public SqlCommand CreateDeleteJobByJobIdCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			var deleteJobByIdSqlCommandText = @"DELETE FROM [Stardust].[Job] 
												   WHERE JobId = @JobId";

			var deleteCommand = new SqlCommand(deleteJobByIdSqlCommandText, sqlConnection);
			deleteCommand.Transaction = sqlTransaction;

			deleteCommand.Parameters.AddWithValue("@JobId",jobId);

			return deleteCommand;
		}

		public SqlCommand CreateSelectAllAliveWorkerNodesCommand(SqlConnection sqlConnection)
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

		public SqlCommand CreateSelectWorkerNodeCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			var selectWorkerNodeUriFromJobCommandText = "SELECT SentToWorkerNodeUri " +
																	"FROM [Stardust].[Job] " +
																	"WHERE JobId = @JobId";

			var selectCommand = new SqlCommand(selectWorkerNodeUriFromJobCommandText, sqlConnection);
			selectCommand.Transaction = sqlTransaction;

			selectCommand.Parameters.AddWithValue("@JobId", jobId);
			return selectCommand;
		}

		public SqlCommand CreateSelectAllItemsInJobQueueCommand(SqlConnection connection)
		{
			const string selectAllItemsInJobQueueCommandText = @"SELECT 
										[JobId],
										[Name],
										[Type],
										[Serialized],
										[CreatedBy],
										[Created]
									FROM [Stardust].[JobQueue]";

			var command = new SqlCommand(selectAllItemsInJobQueueCommandText);
			command.Connection = connection;

			return command;
		}

		public SqlCommand CreateDeleteFromJobQueueCommand(Guid jobId, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
		{
			const string deleteItemFromJobQueueItemCommandText =
					"DELETE FROM [Stardust].[JobQueue] " +
					"WHERE JobId = @JobId";

			var deleteFromJobQueueCommand = new SqlCommand(deleteItemFromJobQueueItemCommandText, sqlConnection);
			deleteFromJobQueueCommand.Parameters.AddWithValue("@JobId", jobId);
			deleteFromJobQueueCommand.Transaction = sqlTransaction;

			return deleteFromJobQueueCommand;
		}


		public SqlCommand CreateDoesJobDetailItemExistsCommand(Guid jobId,
														   SqlConnection sqlConnection)
		{
			const string selectCommandText = @"SELECT COUNT(*) 
											FROM [Stardust].[JobDetail]
											WHERE JobId = @JobId";

			var command = new SqlCommand(selectCommandText, sqlConnection);

			command.Parameters.AddWithValue("@JobId", jobId);

			return command;
		}

		public SqlCommand CreateDoesJobQueueItemExistsCommand(Guid jobId, SqlConnection sqlConnection)
		{
			const string selectCommand = @"SELECT COUNT(*) 
											FROM [Stardust].[JobQueue]
											WHERE JobId = @JobId";

			var sqlCommand = new SqlCommand(selectCommand, sqlConnection);

			sqlCommand.Parameters.AddWithValue("@JobId", jobId);

			return sqlCommand;
		}

		public SqlCommand CreateDoesJobItemExistsCommand(Guid jobId, SqlConnection sqlConnection)
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
