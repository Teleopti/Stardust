/* 
Trunk initiated: 
2011-03-07 
13:03
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David J
--Date: 2011-03-19
--Desc: #14124 - Remove all user settings for reports 
----------------  
TRUNCATE TABLE mart.report_user_setting 
GO 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (317,'7.1.317') 
