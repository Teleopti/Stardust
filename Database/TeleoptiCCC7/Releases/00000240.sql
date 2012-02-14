/* 
Trunk initiated: 
2010-05-04 
08:41
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-05-04  
--Desc: The customers need more space to write text when working with requests
----------------  
ALTER TABLE PersonRequest ALTER COLUMN [Message] nvarchar(2000) null
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (240,'7.1.240') 
