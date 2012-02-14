/* 
Trunk initiated: 
2010-03-31 
08:16
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-04-01  
--Desc: All persons should have a Time Zone specified, setting a default value
----------------  
UPDATE Person SET DefaultTimeZone = 'W. Europe Standard Time' WHERE DefaultTimeZone IS NULL
GO
ALTER TABLE Person ALTER COLUMN [DefaultTimeZone] [nvarchar](50) NOT NULL
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (224,'7.1.224') 
