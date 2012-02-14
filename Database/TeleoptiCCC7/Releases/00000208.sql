/* 
Trunk initiated: 
2010-03-01 
11:44
By: TOPTINET\andersf 
On ANDERSFNC8430 
*/ 
----------------  
--Name: Robin Karlsson
--Date: 2010-03-01
--Desc: Removing "skräpdata" that blocks personal settings in Forecaster
----------------  
DELETE FROM GlobalSettingData WHERE [Key]='Forecaster'
GO 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (208,'7.1.208') 
