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
ALTER TABLE dbo.QueueSource
	DROP CONSTRAINT FK_QueueSource_Person_UpdatedBy
GO
ALTER TABLE dbo.Person SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_QueueSource
	(
	Id uniqueidentifier NOT NULL,
	Version int NOT NULL,
	UpdatedBy uniqueidentifier NOT NULL,
	UpdatedOn datetime NOT NULL,
	QueueMartId int NOT NULL,
	QueueAggId int NOT NULL,
	QueueOriginalId nvarchar(100) NOT NULL,
	DataSourceId int NULL,
	LogObjectName nvarchar(50) NULL,
	Name nvarchar(100) NOT NULL,
	Description nvarchar(100) NULL
	)
GO
ALTER TABLE dbo.Tmp_QueueSource SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.QueueSource)
	 EXEC('INSERT INTO dbo.Tmp_QueueSource (Id, Version, UpdatedBy, UpdatedOn, QueueMartId, QueueAggId, QueueOriginalId, DataSourceId, LogObjectName, Name, Description)
		SELECT Id, Version, UpdatedBy, UpdatedOn, QueueMartId, QueueAggId, CONVERT(nvarchar(100), QueueOriginalId), DataSourceId, LogObjectName, Name, Description FROM dbo.QueueSource WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.QueueSourceCollection
	DROP CONSTRAINT FK_QueueSourceCollection_Workload
GO
DROP TABLE dbo.QueueSource
GO
EXECUTE sp_rename N'dbo.Tmp_QueueSource', N'QueueSource', 'OBJECT' 
GO
ALTER TABLE dbo.QueueSource ADD CONSTRAINT
	PK_QueueSource PRIMARY KEY CLUSTERED 
	(
	Id
	) 

GO
ALTER TABLE dbo.QueueSource ADD CONSTRAINT
	PK_QueueSourceIds UNIQUE NONCLUSTERED 
	(
	QueueMartId,
	QueueAggId,
	QueueOriginalId,
	DataSourceId
	) 

GO
ALTER TABLE dbo.QueueSource WITH NOCHECK ADD CONSTRAINT
	FK_QueueSource_Person_UpdatedBy FOREIGN KEY
	(
	UpdatedBy
	) REFERENCES dbo.Person
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.QueueSourceCollection WITH NOCHECK ADD CONSTRAINT
	FK_QueueSourceCollection_Workload FOREIGN KEY
	(
	QueueSource
	) REFERENCES dbo.QueueSource
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.QueueSourceCollection SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

ALTER TABLE [mart].[dim_queue]
ALTER COLUMN [queue_original_id] nvarchar(100)

GO


--DROP VIEW [dbo].[v_ExternalLogon]
--DROP VIEW [dbo].[v_PersonOrganizationData]
GO

ALTER TABLE [dbo].[ExternalLogOn]
ALTER COLUMN [AcdLogOnOriginalId] nvarchar(100)
GO

