
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Site]') AND name = N'IX_Site_BusinesUnit')
CREATE NONCLUSTERED INDEX [IX_Site_BusinesUnit] ON [dbo].[Site]
(
	[BusinessUnit] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Team]') AND name = N'IX_Team_Site')
CREATE NONCLUSTERED INDEX [IX_Team_Site] ON [dbo].[Team]
(
	[Site] ASC
)
GO

----------------  
--Name: Anders Forsberg
--Date: 2013-12-05
--Desc: Feature #26164 - Add purge of old requests to purge routine
---------------- 
if not exists (select 1 from PurgeSetting where [Key] = 'MonthsToKeepRequests')
	insert into PurgeSetting ([Key], [Value]) values('MonthsToKeepRequests', 120)
GO

----------------  
--Name: David Jonsson
--Date: 2014-02-10
--Desc: bug #26903 - make a more simple version of PersenPeriodWithEndDate for RTA
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Parent_StartDate_Id')
CREATE NONCLUSTERED INDEX [IX_PersonPeriod_Parent_StartDate_Id] ON [dbo].[PersonPeriod]
(
	[Parent] ASC,
	[StartDate] ASC
)
INCLUDE ([Id])
GO

----------------  
--Name: Jiajun Qiu
--Date: 2014-03-25
--Desc: bug #27321 - Remove Grouping Activity
---------------- 
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Activity_GroupingActivity]') AND parent_object_id = OBJECT_ID(N'[dbo].[Activity]'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT [FK_Activity_GroupingActivity]
GO

IF EXISTS ( --column exist
SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Activity') AND name = N'GroupingActivity')
BEGIN
	IF NOT EXISTS ( --but is not part of any index
	SELECT * FROM sys.index_columns ic
	INNER JOIN sys.columns c
	on c.object_id = ic.object_id
	WHERE c.object_id = ic.object_id
	AND c.object_id = OBJECT_ID(N'dbo.Activity')
	AND c.name = N'GroupingActivity'
	AND c.column_id = ic.column_id
	)
	BEGIN
		ALTER TABLE dbo.Activity DROP COLUMN GroupingActivity
	END
END


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GroupingActivity]') AND type in (N'U'))
DROP TABLE [dbo].[GroupingActivity]
GO



