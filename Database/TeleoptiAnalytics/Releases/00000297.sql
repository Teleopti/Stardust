/* 
Trunk initiated: 
2010-10-04 
08:50
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Anders F
--Date: 2010-10-05  
--Desc: Switching all customers to ClientTcpIp as it has been proven in battle now...  
----------------  
update msg.configuration set ConfigurationValue = 'ClientTcpIp'
where ConfigurationId = 14
update msg.configuration set ConfigurationValue = 5000
where ConfigurationId = 11
update msg.configuration set ConfigurationValue = 10000
where ConfigurationId = 4
update msg.configuration set ConfigurationValue = 60000
where ConfigurationId = 10

GO 
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (297,'7.1.297') 
