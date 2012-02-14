/* 
Trunk initiated: 
2010-02-01 
09:22
By: TOPTINET\davidj 
On TELEOPTI625 
*/ 
----------------  
--Name: David J
--Date: 2010-02-01
--Desc: #9265 - remove duplicates from [PersonDayOff]
----------------  
--Delete duplicates
--Keep only the most rescent UpdatedOn
/*
DELETE f
FROM  [dbo].[PersonDayOff] AS f
INNER JOIN [dbo].[PersonDayOff] AS g
ON g.Anchor = f.Anchor AND g.Person = f.Person AND g.Scenario = f.Scenario AND f.UpdatedOn < g.UpdatedOn
*/
DELETE f
FROM  [dbo].[PersonDayOff] AS f
INNER JOIN [dbo].[PersonDayOff] AS g
ON g.Anchor = f.Anchor AND g.Person = f.Person AND g.Scenario = f.Scenario AND f.Id < g.Id

--Add uniqe constraint
ALTER TABLE [dbo].[PersonDayOff] WITH CHECK 
ADD CONSTRAINT UC_PersonAnchorScenario UNIQUE (
[Anchor]
,[Person]
,[Scenario]
)
 
GO 
 

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (197,'7.1.197') 
