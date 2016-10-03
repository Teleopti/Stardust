ALTER TABLE [dbo].[ExternalLogOn] ADD DataSourceName nvarchar(100)
GO
UPDATE [dbo].[ExternalLogOn]
SET DataSourceName = q.LogObjectName
FROM [dbo].[ExternalLogOn] e
INNER JOIN [dbo].[QueueSource] q ON q.DataSourceId=e.DataSourceId
WHERE e.DataSourceName IS NULL
GO