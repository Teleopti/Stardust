DECLARE @sql NVARCHAR(MAX)

SELECT TOP 1 @sql = N'ALTER TABLE [dbo].[WorkflowControlSet] DROP CONSTRAINT ['+dc.NAME+N']'
FROM sys.default_constraints dc
JOIN sys.columns c
    ON c.default_object_id = dc.object_id
WHERE 
    dc.parent_object_id = OBJECT_ID('WorkflowControlSet')
AND c.name = N'AutoGrantOvertimeRequest'
EXEC (@sql)
GO

ALTER TABLE [dbo].[WorkflowControlSet]
DROP COLUMN [AutoGrantOvertimeRequest]
GO
