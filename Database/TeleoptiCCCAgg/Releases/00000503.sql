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
	orig_desc nvarchar(50) NULL,
	log_object_id int NOT NULL,
	orig_queue_id nvarchar(100) NULL,
	display_desc nvarchar(50) NULL
	)  ON [PRIMARY]
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
ALTER TABLE dbo.queue_logg
	DROP CONSTRAINT FK_queue_logg_queues
GO
ALTER TABLE dbo.queue_by_day_logg
	DROP CONSTRAINT FK_queue_by_day_logg_queues
GO
ALTER TABLE dbo.project_logg
	DROP CONSTRAINT FK_project_logg_queues
GO
ALTER TABLE dbo.goal_results
	DROP CONSTRAINT FK_goal_results_queues
GO
ALTER TABLE dbo.agent_logg
	DROP CONSTRAINT FK_agent_logg_queues
GO
DROP TABLE dbo.queues
GO
EXECUTE sp_rename N'dbo.Tmp_queues', N'queues', 'OBJECT' 
GO
ALTER TABLE dbo.queues ADD CONSTRAINT
	PK_queues PRIMARY KEY CLUSTERED 
	(
	queue
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX uix_orig_queue_id ON dbo.queues
	(
	log_object_id,
	orig_queue_id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.queues WITH NOCHECK ADD CONSTRAINT
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
BEGIN TRANSACTION
GO
ALTER TABLE dbo.goal_results ADD CONSTRAINT
	FK_goal_results_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.goal_results SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.project_logg ADD CONSTRAINT
	FK_project_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.project_logg SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.queue_by_day_logg ADD CONSTRAINT
	FK_queue_by_day_logg_queues FOREIGN KEY
	(
	queue
	) REFERENCES dbo.queues
	(
	queue
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.queue_by_day_logg SET (LOCK_ESCALATION = TABLE)
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
