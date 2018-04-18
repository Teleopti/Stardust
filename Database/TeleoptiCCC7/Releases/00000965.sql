-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-18
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

ALTER TABLE dbo.AvailableUnitsInApplicationRole
	DROP CONSTRAINT FK_AvailableUnitsInApplicationRole_BusinessUnit
GO
ALTER TABLE dbo.BusinessUnit SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.AvailableUnitsInApplicationRole
	DROP CONSTRAINT FK_AvailableUnitsInApplicationRole_AvailableData
GO

BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_AvailableUnitsInApplicationRole
	(
	AvailableData uniqueidentifier NOT NULL,
	AvailableBusinessUnit uniqueidentifier NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_AvailableUnitsInApplicationRole SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.AvailableUnitsInApplicationRole)
	 EXEC('INSERT INTO dbo.Tmp_AvailableUnitsInApplicationRole (AvailableData, AvailableBusinessUnit)
		SELECT AvailableData, AvailableBusinessUnit FROM dbo.AvailableUnitsInApplicationRole WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.AvailableUnitsInApplicationRole
GO
EXECUTE sp_rename N'dbo.Tmp_AvailableUnitsInApplicationRole', N'AvailableUnitsInApplicationRole', 'OBJECT' 
GO
ALTER TABLE dbo.AvailableUnitsInApplicationRole ADD CONSTRAINT
	PK_AvailableUnitsInApplicationRole PRIMARY KEY CLUSTERED 
	(
		AvailableData,
		AvailableBusinessUnit
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.AvailableUnitsInApplicationRole ADD CONSTRAINT
	FK_AvailableUnitsInApplicationRole_AvailableData FOREIGN KEY
	(
	AvailableData
	) REFERENCES dbo.AvailableData
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.AvailableUnitsInApplicationRole ADD CONSTRAINT
	FK_AvailableUnitsInApplicationRole_BusinessUnit FOREIGN KEY
	(
	AvailableBusinessUnit
	) REFERENCES dbo.BusinessUnit
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT

-----------------------------------------------------------

DECLARE @DROPINDEXQUERY nvarchar(MAX)
DECLARE @INDEXNAME nvarchar(MAX)

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[KeyPerformanceIndicatorCollection]') 
	AND type in (N'UQ')
	and name like 'UQ%[_][_]%%' -- Look for UQ__XXXXXXXXXX__XXXXXXX
	)

select @INDEXNAME

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.KeyPerformanceIndicatorCollection drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end

ALTER TABLE dbo.KeyPerformanceIndicatorCollection ADD CONSTRAINT
	PK_KeyPerformanceIndicatorCollection PRIMARY KEY CLUSTERED 
	(
		Scorecard,
		KeyPerformanceIndicator
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.KeyPerformanceIndicatorCollection SET (LOCK_ESCALATION = TABLE)
GO

