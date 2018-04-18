-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-18
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

DROP INDEX CIX_MasterActivityCollection_MasterActivity ON dbo.MasterActivityCollection
GO

ALTER TABLE dbo.MasterActivityCollection ADD CONSTRAINT
	PK_MasterActivityCollection PRIMARY KEY CLUSTERED 
	(
		MasterActivity,
		Activity
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.MasterActivityCollection SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------

ALTER TABLE dbo.MultiplicatorDefinitionSetCollection
	DROP CONSTRAINT UQ_MultiplicatorDefinitionSetCollection
GO

ALTER TABLE dbo.MultiplicatorDefinitionSetCollection ADD CONSTRAINT
	PK_MultiplicatorDefinitionSetCollection PRIMARY KEY CLUSTERED 
	(
		Contract,
		MultiplicatorDefinitionSet
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.MultiplicatorDefinitionSetCollection SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
ALTER TABLE dbo.OutlierDates
	DROP CONSTRAINT FK_OutlierDates_Outlier
GO

DROP INDEX CIX_OutlierDates_Parent ON dbo.OutlierDates
GO

CREATE TABLE dbo.Tmp_OutlierDates
	(
		Parent uniqueidentifier NOT NULL,
		Date datetime NOT NULL
	)  ON [PRIMARY]

ALTER TABLE dbo.Tmp_OutlierDates SET (LOCK_ESCALATION = TABLE)

IF EXISTS(SELECT * FROM dbo.OutlierDates)
	 EXEC('INSERT INTO dbo.Tmp_OutlierDates (Parent, Date)
		SELECT Parent, Date FROM dbo.OutlierDates WITH (HOLDLOCK TABLOCKX)');

DROP TABLE dbo.OutlierDates

EXECUTE sp_rename N'dbo.Tmp_OutlierDates', N'OutlierDates', 'OBJECT' 

ALTER TABLE dbo.OutlierDates ADD CONSTRAINT
	PK_OutlierDates PRIMARY KEY CLUSTERED 
	(
		Parent,
		Date
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE dbo.OutlierDates ADD CONSTRAINT
	FK_OutlierDates_Outlier FOREIGN KEY
	(
	Parent
	) REFERENCES dbo.Outlier
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
		
GO
-----------------------------------------------------------
