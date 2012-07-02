----------------  
--Name: David Jonsson
--Date: 2012-06-27
--Desc: Wrong merge
----------------  
--Removed stuff already in release 364

----------------  
--Name: Tamas Balog
--Date: 2012-06-19
--Desc: Add PeriodTime colums to SchedulePeriod table
----------------  
ALTER TABLE dbo.SchedulePeriod ADD
	PeriodTime bigint NULL
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

----------------  
--Name: David Jonsson
--Date: 2012-06-27
--Desc: Make DB-deploy work
----------------  
ALTER TABLE ReadModel.FindPerson
ADD SearchValueId uniqueidentifier null
GO

----------------  
--Name: Anders Forsberg
--Date: 2012-06-29
--Desc: Adding possiblity to purge old messages cause it's been asked for
----------------  
if not exists (select 1 from PurgeSetting where [Key] = 'Message')
begin
	insert into PurgeSetting values ('Message', 10)
end
GO
