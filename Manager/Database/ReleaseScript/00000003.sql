USE [$(DBNAME)]
GO

ALTER TABLE [Stardust].[JobDetail] ADD CONSTRAINT FK_JobId FOREIGN KEY (JobId)
REFERENCES [Stardust].[Job](JobId) ON DELETE CASCADE
GO
