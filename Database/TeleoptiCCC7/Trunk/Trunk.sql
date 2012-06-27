ALTER TABLE ReadModel.FindPerson
ADD  SearchValueId uniqueidentifier NULL
----------------  
--Name: Tamas Balog
--Date: 2012-06-19
--Desc: Add PeriodTime colums to SchedulePeriod table
----------------  

ALTER TABLE dbo.SchedulePeriod ADD
	PeriodTime bigint NULL
GO
ALTER TABLE dbo.SchedulePeriod SET (LOCK_ESCALATION = TABLE)
GO

----------------  
--Name: Tamas Balog
--Date: 2012-06-26
--Desc: Change all FixedStaffPeriodWorkTime contract worktype to FixedStaffNormalWorkType
----------------  

UPDATE dbo.Contract
SET EmploymentType = 0
WHERE EmploymentType = 2
GO