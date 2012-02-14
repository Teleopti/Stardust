/* 
Trunk initiated: 
2010-09-08 
08:32
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
--Re-deploy next two statements, for early birds on 7.1.294!!
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
 

GO

  
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (295,'7.1.295') 
