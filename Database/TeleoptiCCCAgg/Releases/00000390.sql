PRINT 'dummy change to flush Infratest BDs'
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (390,'7.5.390') 
