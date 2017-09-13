USE [$(DBNAME)]
GO

ALTER TABLE stardust.jobqueue
ADD Policy nvarchar(max);

ALTER TABLE stardust.job
ADD Policy nvarchar(max);

GO