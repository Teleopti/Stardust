-----------------
--Name: Anders
--Date: 2014-08-25
--Desc: Dummy to create new version
----------------  
declare @MyDummyVariable int

GO
GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (400,'8.0.400') 
