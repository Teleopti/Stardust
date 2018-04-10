IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OvertimeRequestOpenPeriodSkillType]') AND type in (N'U'))
   DROP TABLE [dbo].[OvertimeRequestOpenPeriodSkillType]

GO

CREATE TABLE [dbo].[OvertimeRequestOpenPeriodSkillType](
	[Id] [uniqueidentifier] NOT NULL ,
	[Parent] [uniqueidentifier] NOT NULL,
	[SkillType] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE  [dbo].[OvertimeRequestOpenPeriodSkillType]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_OvertimeRequestOpenPeriod] FOREIGN KEY([Parent])
REFERENCES [dbo].[OvertimeRequestOpenPeriod] ([Id])
GO

ALTER TABLE  [dbo].[OvertimeRequestOpenPeriodSkillType]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_SkillType] FOREIGN KEY([SkillType])
REFERENCES [dbo].[SkillType] ([Id])
GO 


-----------------------------------------------------------  
-- Name: DevOops
-- Date: 2018-04-09
-- Desc: Fixing PK's with auto generated names. VSTS #75391 
-----------------------------------------------------------

ALTER TABLE  [dbo].[OvertimeRequestOpenPeriodSkillType]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_OvertimeRequestOpenPeriod] FOREIGN KEY([Parent])
REFERENCES [dbo].[OvertimeRequestOpenPeriod] ([Id])
GO

ALTER TABLE  [dbo].[OvertimeRequestOpenPeriodSkillType]  WITH CHECK ADD  CONSTRAINT [FK_OvertimeRequestOpenPeriodSkillType_SkillType] FOREIGN KEY([SkillType])
REFERENCES [dbo].[SkillType] ([Id])
GO

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
