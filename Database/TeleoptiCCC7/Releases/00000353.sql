/*
   2012-02-03
   User: Ola
   Database: Two new Columns on Contract
*/
ALTER TABLE [Contract]
ADD [PositiveDayOffTolerance] [int] NOT NULL DEFAULT ((0)),
	[NegativeDayOffTolerance] [int] NOT NULL DEFAULT ((0))
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (353,'7.1.353') 
