----------------  
--Name: Roger Kratz  
--Date: 2012-01-19
--Desc: New version of Envers. Simplified schedule,
--		  removed duplicated information
----------------  
alter table [Auditing].[MainShift_AUD]
drop column RefId

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (351,'7.1.351') 
