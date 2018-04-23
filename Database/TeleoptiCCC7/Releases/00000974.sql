-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-19
-- Desc: Fixing PK's. VSTS #75391 
-----------------------------------------------------------

EXEC sp_rename N'[dbo].[ExternalPerformance].[PK_ExternalPerformanceInfo]', N'PK_ExternalPerformance', N'INDEX' 
EXEC sp_rename N'[Tenant].[AdminAccessToken].[PK_Tenant.AdminAccessToken]', N'PK_AdminAccessToken', N'INDEX' 
EXEC sp_rename N'[Tenant].[TenantApplicationConfig].[PK_TenantApplicationNhibernateConfig]', N'PK_TenantApplicationConfig', N'INDEX' 

-----------------------------------------------------------

DECLARE @DROPINDEXQUERY nvarchar(MAX)
DECLARE @INDEXNAME nvarchar(MAX)

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[PersonAssociationCheckSum]') 
	AND type in (N'PK')
	and name like 'PK%[_][_]%%' -- Look for PK__XXXXXXXXX__XXXXXXX
	)

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.PersonAssociationCheckSum drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end

ALTER TABLE dbo.PersonAssociationCheckSum ADD CONSTRAINT
	PK_PersonAssociationCheckSum PRIMARY KEY CLUSTERED 
	(
		PersonId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.PersonAssociationCheckSum SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------
DECLARE @DROPINDEXQUERY nvarchar(MAX)
DECLARE @INDEXNAME nvarchar(MAX)

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[OvertimeRequestOpenPeriodSkillType]') 
	AND type in (N'PK')
	and name like 'PK%[_][_]%%' -- Look for PK__XXXXXXXXX__XXXXXXX
	)

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.OvertimeRequestOpenPeriodSkillType drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end

ALTER TABLE dbo.OvertimeRequestOpenPeriodSkillType ADD CONSTRAINT
	PK_OvertimeRequestOpenPeriodSkillType PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.OvertimeRequestOpenPeriodSkillType SET (LOCK_ESCALATION = TABLE)
GO

-----------------------------------------------------------

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod] DROP CONSTRAINT [FK_OvertimeRequestOpenPeriod_WorkflowControlSet]
GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriodSkillType] DROP CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_OvertimeRequestOpenPeriod]
GO

DECLARE @DROPINDEXQUERY nvarchar(MAX)
DECLARE @INDEXNAME nvarchar(MAX)

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[OvertimeRequestOpenPeriod]') 
	AND type in (N'D')
	and name like 'DF%[_][_]%%' -- Look for PK__XXXXXXXXX__XXXXXXX
	)

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.OvertimeRequestOpenPeriod drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end

SET @DROPINDEXQUERY = ''
SET @INDEXNAME = ''

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[OvertimeRequestOpenPeriod]') 
	AND type in (N'PK')
	and name like 'PK%[_][_]%%' -- Look for PK__XXXXXXXXX__XXXXXXX
	)

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.OvertimeRequestOpenPeriod drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end

ALTER TABLE dbo.OvertimeRequestOpenPeriod ADD CONSTRAINT
	PK_OvertimeRequestOpenPeriod PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.OvertimeRequestOpenPeriod SET (LOCK_ESCALATION = TABLE)
GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod] ADD CONSTRAINT [DF_OvertimeRequestOpenPeriod_EnableWorkRuleValidation] DEFAULT  ((0)) FOR [EnableWorkRuleValidation]
GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod]  WITH NOCHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriod_WorkflowControlSet] FOREIGN KEY([Parent])
REFERENCES [dbo].[WorkflowControlSet] ([Id])
GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriod] CHECK CONSTRAINT [FK_OvertimeRequestOpenPeriod_WorkflowControlSet]
GO


ALTER TABLE [dbo].[OvertimeRequestOpenPeriodSkillType]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_OvertimeRequestOpenPeriod] FOREIGN KEY([Parent])
REFERENCES [dbo].[OvertimeRequestOpenPeriod] ([Id])
GO

ALTER TABLE [dbo].[OvertimeRequestOpenPeriodSkillType] CHECK CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_OvertimeRequestOpenPeriod]
GO


