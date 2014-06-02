-----------------
--Name: Anders
--Date: 2014-06-02
--Desc: Dummy to create new version
----------------  
declare @MyDummyVariable int

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (395,'7.5.395') 
