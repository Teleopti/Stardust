-- bug 28436 
UPDATE mart.report
set target = '_blank' WHERE ID = 'D45A8874-57E1-4EB9-826D-E216A4CBC45B'
GO
delete from mart.report where Id = 'BB8C21BA-0756-4DDC-8B26-C9D5715A3443'
GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (397,'8.0.397') 
