/* 
Trunk initiated: 
2011-05-02 
10:07
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Micke D  
--Date: 2011-05-03
--Desc: Dropping unused table
----------------  

truncate table DayOffPlannerRules
drop table DayOffPlannerRules
GO

----------------  
--Name: Devy Developer  
--Date: 2010-xx-xx  
--Desc: Because ...  
----------------  
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (327,'7.1.327') 
