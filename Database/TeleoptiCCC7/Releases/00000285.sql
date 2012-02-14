/* 
Trunk initiated: 
2010-06-16 
08:26
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: David Jonsson
--Date: 2010-06-16
--Desc: Bug #7989
--		An extra fnutt made all KPIs go away
----------------
update dbo.KeyPerformanceIndicator
set ResourceKey = 'KpiAverageHandleTime'
where ResourceKey = 'KpiAverageHandleTime'''
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (285,'7.1.285') 
