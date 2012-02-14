/* 
Trunk initiated: 
2009-11-10 
14:08
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: JN
--Date: 2009-11-11  
--Desc: Make sure all deleted skill´s workloads is marked as deleted and that their data is deleted.  
----------------  
DELETE FROM WorkloadDay
FROM WorkloadDay wd
INNER JOIN WorkloadDayBase wdb ON wd.WorkloadDayBase = wdb.Id
INNER JOIN Workload w ON wdb.Workload = w.Id
INNER JOIN Skill s ON w.Skill = s.Id
WHERE s.IsDeleted = 1

DELETE FROM WorkloadDayTemplate
FROM WorkloadDayTemplate wd
INNER JOIN WorkloadDayBase wdb ON wd.WorkloadDayBase = wdb.Id
INNER JOIN Workload w ON wdb.Workload = w.Id
INNER JOIN Skill s ON w.Skill = s.Id
WHERE s.IsDeleted = 1

DELETE FROM TemplateTaskPeriod
FROM TemplateTaskPeriod wd
INNER JOIN WorkloadDayBase wdb ON wd.Parent = wdb.Id
INNER JOIN Workload w ON wdb.Workload = w.Id
INNER JOIN Skill s ON w.Skill = s.Id
WHERE s.IsDeleted = 1

DELETE FROM OpenHourList
FROM OpenHourList wd
INNER JOIN WorkloadDayBase wdb ON wd.Parent = wdb.Id
INNER JOIN Workload w ON wdb.Workload = w.Id
INNER JOIN Skill s ON w.Skill = s.Id
WHERE s.IsDeleted = 1

DELETE FROM WorkloadDayBase
FROM WorkloadDayBase wdb
INNER JOIN Workload w ON wdb.Workload = w.Id
INNER JOIN Skill s ON w.Skill = s.Id
WHERE s.IsDeleted = 1

UPDATE Workload
SET IsDeleted = 1
FROM Workload w
INNER JOIN Skill s ON w.Skill = s.Id
WHERE s.IsDeleted = 1


----------------  
--Name: MD
--Date: 2009-11-19  
--Desc: Added two columns to contract table  
----------------  
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Contract ADD
	PositivePeriodWorkTimeTolerance bigint NOT NULL CONSTRAINT DF_Contract_PositivePeriodWorkTimeTolerance DEFAULT 0,
	NegativePeriodWorkTimeTolerance bigint NOT NULL CONSTRAINT DF_Contract_NegativePeriodWorkTimeTolerance DEFAULT 0
GO
COMMIT

----------------  
--Name: JN
--Date: 2009-11-23  
--Desc: Make sure the default scenario is reportable
----------------  
UPDATE dbo.Scenario 
SET EnableReporting=1
WHERE DefaultScenario=1
GO  
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (176,'7.0.176') 
