/* 
Trunk initiated: 
2009-10-12 
08:35
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Jonas Nordh
--Date: 2009-10-12  
--Desc: Hide Agent scorecard in client report list  
----------------  
UPDATE mart.report 
SET visible=0
WHERE report_id=6 
GO 
 
EXEC mart.sys_crossdatabaseview_load  
GO  
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (160,'7.0.160') 
