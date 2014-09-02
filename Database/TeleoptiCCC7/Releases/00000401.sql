-----------------
--Name: Anders
--Date: 2014-09-02
--Desc: Dummy to create new version
----------------  
declare @MyDummyVariable int

GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (401,'8.0.401') 
