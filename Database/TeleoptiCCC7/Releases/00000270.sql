/* 
Trunk initiated: 
2010-05-28 
09:52
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 

----------------  
--Name: Tamas Balog  
--Date: 2010-06-01  
--Desc: Bugfix 10775: rename Schedules/Requests/Automatic updates to Schedules/Requests/Approve   
----------------  
update dbo.ApplicationFunction
set FunctionCode = 'xxApprove', FunctionDescription = 'Approve'
where ForeignId = '0022' and ForeignSource='Raptor'

 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (270,'7.1.270') 
