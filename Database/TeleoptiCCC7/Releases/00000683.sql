--delete corrupted data
DELETE FROM [Stardust].[JobDetail] WHERE JobId 
NOT IN (SELECT JobId FROM [Stardust].[Job])

ALTER TABLE [Stardust].[JobDetail] ADD CONSTRAINT FK_JobId FOREIGN KEY (JobId)
REFERENCES [Stardust].[Job](JobId) ON DELETE CASCADE
GO
