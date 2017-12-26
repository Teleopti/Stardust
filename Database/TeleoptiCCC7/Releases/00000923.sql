IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE [name] = 'OvertimeRequestMaximumTimeHandleType' and OBJECT_NAME(object_id) = 'WorkflowControlSet')
ALTER TABLE [dbo].[WorkflowControlSet] ADD OvertimeRequestMaximumTimeHandleType INT

GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE [name] = 'OvertimeRequestMaximumTime' and OBJECT_NAME(object_id) = 'WorkflowControlSet')
ALTER TABLE [dbo].[WorkflowControlSet] ADD OvertimeRequestMaximumTime BIGINT
GO