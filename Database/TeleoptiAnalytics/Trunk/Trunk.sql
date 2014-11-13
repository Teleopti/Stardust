----------------  
--Name: Mathias Stenbom
--Date: 2014-11-13
--Desc: changes to actual agent state
----------------  

ALTER TABLE [RTA].[ActualAgentState] ALTER COLUMN [OriginalDataSourceId] NVARCHAR(50) NULL
GO

ALTER TABLE [RTA].[ActualAgentState] ADD [BusinessUnitId] uniqueidentifier NULL
GO
