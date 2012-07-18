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


----------------  
--Name: Robin Karlsson
--Date: 2012-07-18
--Desc: Removing duplicate days from Preferences and saves the latest one. Also create a new constraint to avoid future duplicates.
----------------  

CREATE TABLE #double_preferences (person uniqueidentifier, restrictiondate datetime, lastupdated datetime)
INSERT INTO  #double_preferences
SELECT p1.Person,p1.RestrictionDate,MAX(UpdatedOn) FROM PreferenceDay p1 GROUP BY p1.Person,p1.RestrictionDate HAVING COUNT(p1.Person) > 1

DELETE ActivityRestriction FROM ActivityRestriction ar INNER JOIN PreferenceRestriction pr ON ar.Parent=pr.Id INNER JOIN PreferenceDay p ON p.Id=pr.Id INNER JOIN #double_preferences d ON d.person=p.Person AND d.restrictiondate=p.RestrictionDate AND d.lastupdated<>p.UpdatedOn
DELETE PreferenceRestriction FROM PreferenceRestriction pr INNER JOIN PreferenceDay p ON p.Id=pr.Id INNER JOIN #double_preferences d ON d.person=p.Person AND d.restrictiondate=p.RestrictionDate AND d.lastupdated<>p.UpdatedOn
DELETE PreferenceDay FROM PreferenceDay p INNER JOIN #double_preferences d ON d.person=p.Person AND d.restrictiondate=p.RestrictionDate AND d.lastupdated<>p.UpdatedOn
DROP TABLE #double_preferences

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name=N'uq_preference_day_per_agent')
	BEGIN
		CREATE UNIQUE NONCLUSTERED INDEX uq_preference_day_per_agent ON dbo.PreferenceDay
			(
			person,
			restrictiondate
			)
	END
	
GO