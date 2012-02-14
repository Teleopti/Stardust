/* 
Trunk initiated: 
2009-12-10 
08:25
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: david jonsson
--Date: 2009-12-14
--Desc: Clean out obsolete/broken data  
----------------
--Delete invalid Preferences
CREATE TABLE #PrefIds (id UniqueIdentifier )

INSERT INTO #PrefIds
SELECT Id FROM dbo.PreferenceRestriction PR
WHERE				  ShiftCategory         IS NULL
AND                   DayOffTemplate        IS NULL
AND                   StartTimeMinimum      IS NULL
AND                   StartTimeMaximum      IS NULL
AND                   EndTimeMinimum        IS NULL
AND                   EndTimeMaximum		IS NULL
AND                   WorkTimeMinimum		IS NULL
AND                   WorkTimeMaximum       IS NULL

DELETE PR
FROM dbo.PreferenceRestriction PR
INNER JOIN #PrefIds Del
ON PR.id = Del.id

DELETE PD
FROM dbo.PreferenceDay PD
INNER JOIN #PrefIds Del
ON PD.id = Del.id

DROP TABLE #PrefIds

 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (183,'7.0.183') 
