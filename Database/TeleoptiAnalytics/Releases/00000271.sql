/* 
Trunk initiated: 
2010-06-02 
10:35
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-06-03
--Desc: Making the address field bigger to contain dns addresses, not just IP
----------------  
ALTER TABLE msg.[Address] ALTER COLUMN [Address] NVARCHAR(255) NOT NULL
GO 
GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (271,'7.1.271') 
