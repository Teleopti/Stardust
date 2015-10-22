--Earlier glitch from 625 and that Merge-proc... Make sure this one is permanently removed to block deadlocks
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UQ_PersonAssignment_Date_Scenario_Person]') AND type_desc LIKE 'UNIQUE_CONSTRAINT')
ALTER TABLE [dbo].[PersonAssignment] DROP CONSTRAINT [UQ_PersonAssignment_Date_Scenario_Person]