/* 
Trunk initiated: 
2010-04-12 
15:07
By: TOPTINET\andersf 
On ANDERSFNC8430 
*/ 
----------------  
--Name: Ola Håkansson  
--Date: 2010-04-13 
--Desc: Adding Priority And OverStaffingFaktor to Skill  
----------------  
ALTER TABLE dbo.Skill ADD
Priority int NULL,
OverstaffingFactor float NULL

GO

UPDATE  dbo.Skill set Priority = 4, OverstaffingFactor = .5

GO 

ALTER TABLE Skill ALTER COLUMN Priority int NOT NULL
ALTER TABLE Skill ALTER COLUMN OverstaffingFactor float NOT NULL 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (230,'7.1.230') 
