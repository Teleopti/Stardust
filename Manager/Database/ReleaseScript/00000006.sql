USE [$(DBNAME)]
GO

ALTER TABLE stardust.WorkerNode
ADD PingResult Bit NULL DEFAULT(0);

GO