
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