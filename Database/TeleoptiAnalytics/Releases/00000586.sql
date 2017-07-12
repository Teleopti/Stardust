	DROP INDEX [IX_HangFire_JobQueue_QueueAndFetchedAt] ON [HangFire].[JobQueue];

	ALTER TABLE [HangFire].[JobQueue] ALTER COLUMN [Queue] NVARCHAR (50) NOT NULL;

	CREATE NONCLUSTERED INDEX [IX_HangFire_JobQueue_QueueAndFetchedAt] ON [HangFire].[JobQueue] (
        [Queue] ASC,
        [FetchedAt] ASC
    );

	ALTER TABLE [HangFire].[Server] DROP CONSTRAINT [PK_HangFire_Server]

	ALTER TABLE [HangFire].[Server] ALTER COLUMN [Id] NVARCHAR (100) NOT NULL;

	ALTER TABLE [HangFire].[Server] ADD  CONSTRAINT [PK_HangFire_Server] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	);

DECLARE @CURRENT_SCHEMA_VERSION INT;
SET @CURRENT_SCHEMA_VERSION = 5;
UPDATE [HangFire].[Schema] SET [Version] = @CURRENT_SCHEMA_VERSION
IF @@ROWCOUNT = 0 
	INSERT INTO [HangFire].[Schema] ([Version]) VALUES (@CURRENT_SCHEMA_VERSION)