IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn')
DROP INDEX [IX_PersonAssignment_Scenario_UpdatedBy_UpdatedOn] ON [dbo].[PersonAssignment]

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'IX_PersonAssignment_Date')
DROP INDEX [IX_PersonAssignment_Date] ON [dbo].[PersonAssignment]
