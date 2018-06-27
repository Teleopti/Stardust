-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-06-27
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------
ALTER TABLE dbo.ReplyOptions
	DROP CONSTRAINT FK_PushMessage_ReplyOption
GO
ALTER TABLE dbo.PushMessage SET (LOCK_ESCALATION = TABLE)
GO
CREATE TABLE dbo.Tmp_ReplyOptions
	(
	id uniqueidentifier NOT NULL,
	elt nvarchar(255) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_ReplyOptions SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.ReplyOptions)
	 EXEC('INSERT INTO dbo.Tmp_ReplyOptions (id, elt)
		SELECT id, elt FROM dbo.ReplyOptions WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.ReplyOptions
GO
EXECUTE sp_rename N'dbo.Tmp_ReplyOptions', N'ReplyOptions', 'OBJECT' 
GO
ALTER TABLE dbo.ReplyOptions ADD CONSTRAINT
	PK_ReplyOptions PRIMARY KEY NONCLUSTERED 
	(
	id,
	elt
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX CIX_ReplyOptions_Id ON dbo.ReplyOptions
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.ReplyOptions ADD CONSTRAINT
	FK_PushMessage_ReplyOption FOREIGN KEY
	(
	id
	) REFERENCES dbo.PushMessage
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
