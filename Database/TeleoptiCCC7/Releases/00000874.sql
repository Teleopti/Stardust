-- The column CheckStaffingForOvertimeRequest was added in development, it may not be created in product environment
-- So the existing check here is necessary.
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'CheckStaffingForOvertimeRequest' AND Object_ID = Object_ID(N'dbo.[WorkflowControlSet]'))
BEGIN
    -- Need drop constraint first
    DECLARE @sql nvarchar(200) = ''
    SELECT @sql = N'ALTER TABLE dbo.[WorkflowControlset] DROP CONSTRAINT [' + dc.NAME + N']'
      FROM sys.default_constraints dc
      JOIN sys.columns col ON col.default_object_id = dc.object_id
     WHERE dc.parent_object_id = OBJECT_ID('dbo.[WorkflowControlset]')
       AND col.name = 'CheckStaffingForOvertimeRequest'

    IF @sql <> '' EXEC (@sql)

    ALTER TABLE dbo.[WorkflowControlSet] DROP COLUMN CheckStaffingForOvertimeRequest
END
