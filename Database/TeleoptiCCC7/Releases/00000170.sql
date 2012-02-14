/* 
Trunk initiated: 
2009-11-03 
13:45
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 


----------------  
--Name: Henry Greijer
--Date: 2009-11-04
--Desc: Hard delete WorkloadDays whos Workload has IsDeleted set to 1
----------------  

DELETE FROM WorkloadDay 
	WHERE WorkloadDayBase IN
		(
			SELECT Id FROM WorkloadDayBase
				WHERE Workload IN
					(
						SELECT Id FROM Workload WHERE IsDeleted = 1
					)
		)

DELETE FROM WorkloadDayTemplate
	WHERE WorkloadDayBase IN
		(
			SELECT Id FROM WorkloadDayBase
				WHERE Workload IN
					(
						SELECT Id FROM Workload WHERE IsDeleted = 1
					)
		)

DELETE FROM TemplateTaskPeriod
	WHERE Parent IN
		(
			SELECT Id FROM WorkloadDayBase
				WHERE Workload IN
					(
						SELECT Id FROM Workload WHERE IsDeleted = 1
					)
		)

DELETE FROM OpenHourList
	WHERE Parent IN
		(
			SELECT Id FROM WorkloadDayBase
				WHERE Workload IN
					(
						SELECT Id FROM Workload WHERE IsDeleted = 1
					)
		)

DELETE FROM WorkloadDayBase
				WHERE Workload IN
					(
						SELECT Id FROM Workload WHERE IsDeleted = 1
					)

 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (170,'7.0.170') 
