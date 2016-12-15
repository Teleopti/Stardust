----------------  
--Name: Mingdi
--Date: 2016-12-14
--Desc: Add default value 15min for AbsenceRequestExpiredThreshold in WorkflowControlSet. bug #41931
----------------  
UPDATE dbo.WorkflowControlSet
SET AbsenceRequestExpiredThreshold=15
WHERE AbsenceRequestExpiredThreshold IS NULL
GO

ALTER TABLE dbo.WorkflowControlSet
AlTER COLUMN AbsenceRequestExpiredThreshold int NOT NULL 
GO


