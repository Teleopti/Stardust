/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE mart.Tmp_dim_queue_excluded
	(
	queue_original_id nvarchar(100) NOT NULL,
	datasource_id smallint NOT NULL
	) 
GO
ALTER TABLE mart.Tmp_dim_queue_excluded SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM mart.dim_queue_excluded)
	 EXEC('INSERT INTO mart.Tmp_dim_queue_excluded (queue_original_id, datasource_id)
		SELECT CONVERT(nvarchar(100), queue_original_id), datasource_id FROM mart.dim_queue_excluded WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE mart.dim_queue_excluded
GO
EXECUTE sp_rename N'mart.Tmp_dim_queue_excluded', N'dim_queue_excluded', 'OBJECT' 
GO
ALTER TABLE mart.dim_queue_excluded ADD CONSTRAINT
	PK_dim_queue_excluded PRIMARY KEY CLUSTERED 
	(
	queue_original_id,
	datasource_id
	)

GO
COMMIT

ALTER TABLE [mart].[dim_queue]
ALTER COLUMN [queue_original_id] nvarchar(100)

GO

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.queues
	DROP CONSTRAINT FK_queues_log_object
GO
ALTER TABLE dbo.log_object SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_queues
	(
	queue int NOT NULL IDENTITY (1, 1),
	orig_desc nvarchar(100) NULL,
	log_object_id int NOT NULL,
	orig_queue_id nvarchar(100) NULL,
	display_desc nvarchar(100) NULL
	)
GO
ALTER TABLE dbo.Tmp_queues SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_queues ON
GO
IF EXISTS(SELECT * FROM dbo.queues)
	 EXEC('INSERT INTO dbo.Tmp_queues (queue, orig_desc, log_object_id, orig_queue_id, display_desc)
		SELECT queue, orig_desc, log_object_id, CONVERT(nvarchar(100), orig_queue_id), display_desc FROM dbo.queues WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_queues OFF
GO
ALTER TABLE dbo.agent_logg
	DROP CONSTRAINT FK_agent_logg_queues
GO
ALTER TABLE dbo.queue_logg
	DROP CONSTRAINT FK_queue_logg_queues
GO
DROP TABLE dbo.queues
GO
EXECUTE sp_rename N'dbo.Tmp_queues', N'queues', 'OBJECT' 
GO
ALTER TABLE dbo.queues ADD CONSTRAINT
	PK_queues PRIMARY KEY CLUSTERED 
	(
	queue
	)

GO
CREATE UNIQUE NONCLUSTERED INDEX uix_orig_queue_id ON dbo.queues
	(
	log_object_id,
	orig_queue_id
	)
GO
ALTER TABLE dbo.queues ADD CONSTRAINT
	FK_queues_log_object FOREIGN KEY
	(
	log_object_id
	) REFERENCES dbo.log_object
	(
	log_object_id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.queue_logg ADD CONSTRAINT
	FK_queue_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.queue_logg SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.agent_logg ADD CONSTRAINT
	FK_agent_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.agent_logg SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


ALTER TABLE [mart].[dim_acd_login]
ALTER COLUMN [acd_login_original_id] nvarchar(100)

GO


ALTER TABLE [dbo].[agent_info]
ALTER COLUMN [orig_agent_id] nvarchar(100)

ALTER TABLE [dbo].[agent_info]
ALTER COLUMN [Agent_name] nvarchar(100)

ALTER TABLE [dbo].[agent_logg]
ALTER COLUMN [agent_name] nvarchar(100)
GO

ALTER TABLE [stage].[stg_queue]
ALTER COLUMN [queue_name] nvarchar(100)
GO

ALTER TABLE [stage].[stg_acd_login_person]
ALTER COLUMN [acd_login_code] nvarchar(100)
GO

TRUNCATE TABLE [stage].[stg_queue_workload]
ALTER TABLE [stage].[stg_queue_workload] DROP CONSTRAINT [PK_stg_queue_workload]
GO
ALTER TABLE [stage].[stg_queue_workload]
ALTER COLUMN [queue_code] nvarchar(100) NOT NULL
GO
ALTER TABLE [stage].[stg_queue_workload] ADD  CONSTRAINT [PK_stg_queue_workload] PRIMARY KEY CLUSTERED 
(
	[queue_code] ASC,
	[workload_code] ASC,
	[log_object_data_source_id] ASC,
	[log_object_name] ASC
)
GO