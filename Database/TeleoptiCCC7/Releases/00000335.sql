----------------  
--Name: DavidJ
--Date: 2011-09-13
--Desc: Azure needs Clusterded index, already added on 334 (IF EXIST in this release)
----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[WorkflowControlSetAllowedAbsences]') AND name = N'CIX_WorkflowControlSetAllowedAbsences_WorkflowControlSet')
CREATE CLUSTERED INDEX [CIX_WorkflowControlSetAllowedAbsences_WorkflowControlSet]
ON [dbo].[WorkflowControlSetAllowedAbsences] 
(
	[WorkflowControlSet] ASC
)
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (335,'7.1.335') 
