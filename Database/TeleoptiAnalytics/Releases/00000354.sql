/*
20120207 AF: Purge old data
*/





GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (354,'7.1.354') 
