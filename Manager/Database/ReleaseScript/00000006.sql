USE [$(DBNAME)]
GO

ALTER TABLE stardust.WorkerNode
ADD PingResult Bit not null DEFAULT 0

GO