-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-17
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

DROP INDEX CIX_AvailableUnitsInApplicationRole_AvailableData ON dbo.AvailableUnitsInApplicationRole
GO

ALTER TABLE dbo.AvailableUnitsInApplicationRole ADD CONSTRAINT
	PK_AvailableUnitsInApplicationRole PRIMARY KEY CLUSTERED 
	(
	AvailableData
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO



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
