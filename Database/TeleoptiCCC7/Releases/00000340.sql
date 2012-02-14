ALTER TABLE dbo.Person ADD
	FirstDayOfWeek int NOT NULL CONSTRAINT DF_Person_FirstDayOfWeek DEFAULT 1

GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (340,'7.1.340')  
