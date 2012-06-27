----------------  
--Name: David Jonsson
--Date: 2012-06-27
--Desc: Wrong merge
----------------  
--Removed stuff already in release 364

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