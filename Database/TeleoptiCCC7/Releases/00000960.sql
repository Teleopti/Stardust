-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-10
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------
ALTER TABLE [dbo].[AccessibilityDates] DROP CONSTRAINT [FK_AccessibilityDates_WorkShiftRuleSet]

SELECT RuleSet, Date
	into #SaveData
	FROM dbo.AccessibilityDates
	where Date is not null
	
DROP TABLE dbo.AccessibilityDates
GO

CREATE TABLE dbo.AccessibilityDates
	(
		RuleSet uniqueidentifier NOT NULL,
		Date datetime NOT NULL
	)  ON [PRIMARY]
ALTER TABLE dbo.AccessibilityDates SET (LOCK_ESCALATION = TABLE)
GO

insert into dbo.AccessibilityDates
select RuleSet, Date from #SaveData

ALTER TABLE dbo.AccessibilityDates ADD CONSTRAINT
PK_AccessibilityDates PRIMARY KEY CLUSTERED 
(
	RuleSet,
	Date
) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE [dbo].[AccessibilityDates]  WITH CHECK ADD  CONSTRAINT [FK_AccessibilityDates_WorkShiftRuleSet] FOREIGN KEY([RuleSet])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
ALTER TABLE [dbo].[AccessibilityDates] CHECK CONSTRAINT [FK_AccessibilityDates_WorkShiftRuleSet]

drop table #SaveData

GO


-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-10
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

ALTER TABLE [dbo].[AccessibilityDaysOfWeek] DROP CONSTRAINT [FK_AccessibilityDaysOfWeek_WorkShiftRuleSet]

SELECT RuleSet, DayOfWeek
	into #SaveData
	FROM dbo.AccessibilityDaysOfWeek
	where DayOfWeek is not null
	
DROP TABLE dbo.AccessibilityDaysOfWeek
GO

CREATE TABLE [dbo].[AccessibilityDaysOfWeek](
	[RuleSet] [uniqueidentifier] NOT NULL,
	[DayOfWeek] [int] NOT NULL,
) ON [PRIMARY]
GO

insert into dbo.AccessibilityDaysOfWeek
select RuleSet, DayOfWeek from #SaveData

ALTER TABLE dbo.AccessibilityDaysOfWeek ADD CONSTRAINT
PK_AccessibilityDaysOfWeek PRIMARY KEY CLUSTERED 
(
	RuleSet,
	DayOfWeek
) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE [dbo].[AccessibilityDaysOfWeek]  WITH CHECK ADD  CONSTRAINT [FK_AccessibilityDaysOfWeek_WorkShiftRuleSet] FOREIGN KEY([RuleSet])
REFERENCES [dbo].[WorkShiftRuleSet] ([Id])
GO

ALTER TABLE [dbo].[AccessibilityDaysOfWeek] CHECK CONSTRAINT [FK_AccessibilityDaysOfWeek_WorkShiftRuleSet]
drop table #SaveData
GO



-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-10
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

DECLARE @DROPINDEXQUERY nvarchar(MAX)
DECLARE @INDEXNAME nvarchar(MAX)

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[ApplicationFunctionInRole]') 
	AND type in (N'UQ')
	and name like 'UQ%[_][_]%%' -- Look for UQ__XXXXXXXXX__XXXXXXX
	)

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.ApplicationFunctionInRole drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end

ALTER TABLE dbo.ApplicationFunctionInRole ADD CONSTRAINT
	PK_ApplicationFunctionInRole PRIMARY KEY CLUSTERED 
	(
		ApplicationRole,
		ApplicationFunction
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

