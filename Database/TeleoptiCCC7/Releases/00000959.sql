-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-09
-- Desc: Fixing PK's with auto generated names. VSTS #75391 
-----------------------------------------------------------
DECLARE @DROPINDEXQUERY nvarchar(MAX)
DECLARE @INDEXNAME nvarchar(MAX)

set @INDEXNAME = (
	SELECT name FROM sys.objects 
	WHERE parent_object_id = OBJECT_ID(N'[ExternalLogOnCollection]') 
	AND type in (N'UQ')
	and name like 'UQ%[_][_]%%' -- Look for UQ__ExternalLogOnCol__XXXXXXX
	)

select @INDEXNAME

if (@INDEXNAME <> '')
begin
	SET @DROPINDEXQUERY = N'alter table dbo.ExternalLogOnCollection drop constraint [' + @INDEXNAME + ']'
	print 'Dropping index with auto-generated name: ' + @INDEXNAME
	EXEC sp_executeSQL @DROPINDEXQUERY;
end

ALTER TABLE dbo.ExternalLogOnCollection ADD CONSTRAINT
	PK_ExternalLogOnCollection PRIMARY KEY CLUSTERED 
	(
		PersonPeriod,
		ExternalLogOn
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-10
-- Desc: Fixing PK's with auto generated names. VSTS #75391 
-----------------------------------------------------------
SELECT BudgetGroup, Skill
  FROM dbo.SkillCollection
  group by BudgetGroup, Skill
having count(*) > 1

if (@@ROWCOUNT <> 0)
begin
	ALTER TABLE [dbo].[SkillCollection] DROP CONSTRAINT [FK_SkillCollection_Skill]
	ALTER TABLE [dbo].[SkillCollection] DROP CONSTRAINT [FK_SkillCollection_BudgetGroup]

	SELECT distinct BudgetGroup, Skill
	into #SaveData
	  FROM dbo.SkillCollection
	
	delete from dbo.SkillCollection
	
	insert into dbo.SkillCollection
	select BudgetGroup, Skill from #SaveData

	ALTER TABLE [dbo].[SkillCollection]  WITH CHECK ADD  CONSTRAINT [FK_SkillCollection_BudgetGroup] FOREIGN KEY([BudgetGroup])
	REFERENCES [dbo].[BudgetGroup] ([Id])
	ALTER TABLE [dbo].[SkillCollection] CHECK CONSTRAINT [FK_SkillCollection_BudgetGroup]
	ALTER TABLE [dbo].[SkillCollection]  WITH CHECK ADD  CONSTRAINT [FK_SkillCollection_Skill] FOREIGN KEY([Skill])
	REFERENCES [dbo].[Skill] ([Id])
	ALTER TABLE [dbo].[SkillCollection] CHECK CONSTRAINT [FK_SkillCollection_Skill]

	drop table #SaveData
end

ALTER TABLE dbo.SkillCollection ADD CONSTRAINT
	PK_SkillCollection PRIMARY KEY NONCLUSTERED 
	(
		BudgetGroup,
		Skill
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.SkillCollection SET (LOCK_ESCALATION = TABLE)
GO

