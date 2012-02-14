/* 
Trunk initiated: 
2011-04-13 
17:09
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson 
--Date: 2011-04-19
--Desc: Enable Hyphen in Agg-DB-name
----------------  
UPDATE mart.sys_crossdatabaseview
SET View_Definition = REPLACE(View_Definition,'FROM $$$target$$$.dbo','FROM [$$$target$$$].dbo')
 
GO 
 

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (324,'7.1.324') 
