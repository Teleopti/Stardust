/* 
Trunk initiated: 
2010-02-03 
16:09
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Zoë  
--Date: 2010-02-08  
--Desc: Adding MustHavePreference  
---------------- 
ALTER TABLE SchedulePeriod ADD MustHavePreference int
GO
UPDATE SchedulePeriod SET MustHavePreference = 0
GO
ALTER TABLE SchedulePeriod ALTER Column MustHavePreference int NOT NULL
GO
ALTER TABLE PreferenceDay ADD MustHave bit
GO
UPDATE PreferenceDay SET MustHave = 0
GO
ALTER TABLE PreferenceDay ALTER Column MustHave bit NOT NULL 
GO 
 
 ----------------  
--Name: Peter  
--Date: 2010-02-10  
--Desc: Deleting Tracker Overtime and Comptime 
---------------- 
delete from PersonAccountDay where PersonAccount in
(
           select pa.Id from Absence a
           inner join personaccount pa
           on a.Id = pa.TrackingAbsence
           where a.Tracker in (2,3)
)

GO

delete from PersonAccountTime where PersonAccount in
(
           select pa.Id from Absence a
           inner join personaccount pa
           on a.Id = pa.TrackingAbsence
           where a.Tracker in (2,3)
)

GO

delete PersonAccount where Id in
(
           select pa.Id from Absence a
           inner join personaccount pa
           on a.Id = pa.TrackingAbsence
           where a.Tracker in (2,3)
)

GO

update Absence set Tracker = 255 
where Tracker in (2,3)

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (206,'7.1.206') 
