IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn')
DROP INDEX [IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn] ON [dbo].[PersonAssignment] WITH ( ONLINE = OFF )

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Date')
DROP INDEX [IX_PersonAssignment_Date] ON [dbo].[PersonAssignment] WITH ( ONLINE = OFF )

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UQ_PersonAssignment_Date_Scenario_Person]') AND type_desc LIKE 'UNIQUE_CONSTRAINT')
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [UQ_PersonAssignment_Date_Scenario_Person]

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'CIX_PersonAssignment_PersonDate_Scenario')
DROP INDEX [CIX_PersonAssignment_PersonDate_Scenario] ON [dbo].[PersonAssignment] WITH ( ONLINE = OFF )

CREATE UNIQUE CLUSTERED INDEX [CIX_PersonAssignment_PersonDate_Scenario] ON [dbo].[PersonAssignment]
(
	[Person] ASC,
	[Date] ASC,
	[Scenario] ASC
)
GO