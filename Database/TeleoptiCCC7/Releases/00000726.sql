CREATE TABLE dbo.RequestStrategySettings
	(
	Setting varchar(200) NOT NULL,
	value int NOT NULL
	)
GO
ALTER TABLE dbo.RequestStrategySettings ADD CONSTRAINT
	PK_RequestStrategySettings PRIMARY KEY CLUSTERED 
	(
	Setting
	)
GO
INSERT INTO RequestStrategySettings values('AbsenceNearFuture', 2)
INSERT INTO RequestStrategySettings values('AbsenceNearFutureTime', 20)
INSERT INTO RequestStrategySettings values('AbsenceFarFutureTime', 60)