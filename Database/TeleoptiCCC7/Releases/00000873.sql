ALTER TABLE dbo.[WorkflowControlSet] ADD [AutoGrantOvertimeRequest] BIT NOT NULL DEFAULT 0
GO

ALTER TABLE dbo.[WorkflowControlSet] ADD [CheckStaffingForOvertimeRequest] BIT NOT NULL DEFAULT 0
GO

UPDATE [WorkflowControlSet] SET [AutoGrantOvertimeRequest] = 0, [CheckStaffingForOvertimeRequest] = 0
GO
