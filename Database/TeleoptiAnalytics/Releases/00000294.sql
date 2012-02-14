/* 
Trunk initiated: 
2010-09-07 
09:18
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2010-xx-xx  
--Desc: Because ...  
----------------  

  
--Added after 7.1.294 database build!!!
--Needs to manually be deployed to test-environment
----------------  
--Name: KJ,OH
--Date: 2010-09-14
--Desc: # 11587 - Interval to must default to 24:00 instead of 00:15
----------------  
UPDATE mart.report_control_collection
SET default_value='-99'
WHERE control_id=13 AND default_value='287'

----------------  
--Name: DJ+JN
--Date: 2010-09-20
--Desc: # 11825 - Scenario Drop-Down needs to default to "Default Scenario"
----------------  
UPDATE mart.report_control_collection
SET default_value = 0
WHERE control_id=14

GO
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (294,'7.1.294') 
