/* 
Trunk initiated: 
2009-10-12 
14:51
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Anders F
--Date: 2009-10-12  
--Desc: Reduce risk of loosing RTA events  
----------------  
UPDATE msg.configuration
SET ConfigurationValue = 5
WHERE ConfigurationId in (11,12)
 
GO 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (161,'7.0.161') 
