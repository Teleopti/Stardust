/* 
Trunk initiated: 
2010-04-08 
09:00
By: TOPTINET\andersf 
On ANDERSFNC8430 
*/ 
----------------  
--Name: Ola Håkansson 
--Date: 2010-04-08  
--Desc: When creating a new PersonRotation and not changing the StartWeek, 1 gets saved to database instead of 0  
-- Bug 10031
----------------  
UPDATE [PersonRotation]
  SET StartDay = 0 WHERE StartDay = 1 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (227,'7.1.227') 
