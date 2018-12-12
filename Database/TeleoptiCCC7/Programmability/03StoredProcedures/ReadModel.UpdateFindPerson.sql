IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateFindPerson]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdateFindPerson]
GO

CREATE PROCEDURE [ReadModel].[UpdateFindPerson]
@persons nvarchar(max)
WITH EXECUTE AS OWNER

-- =============================================
-- Author:		Ola
-- Create date: 2011-12-19
-- Description:	Updates the read model for finding persons
-- =============================================
-- Date			Who	Description
-- 2012-0x-xx	DJ	Added a parameter which persons we should update
-- 2012-11-18	DJ	#21539 - Speed up initial load 400 sec => 6 sec
-- 2013-02-20	RobinK	Faulty update with team id in some cases.
-- =============================================
AS
-- exec [ReadModel].[UpdateFindPerson] 'B8F5BE96-2C4D-44B5-A5DB-9C6400BB5860'
-- exec [ReadModel].[UpdateFindPerson] '00000000-0000-0000-0000-000000000000'
SET NOCOUNT ON
CREATE TABLE #ids (
	person uniqueidentifier NOT NULL
)

CREATE TABLE #CurrentPeriod (
	Parent  uniqueidentifier NOT NULL,
	Team	uniqueidentifier NOT NULL
)

CREATE TABLE #last (
	Parent  uniqueidentifier NOT NULL,
	Team    uniqueidentifier NOT NULL,
	EndDate DateTime NOT NULL
)


IF @persons = '00000000-0000-0000-0000-000000000000'  --"EveryBody"
--Flush and re-load everybody
BEGIN
	TRUNCATE TABLE [ReadModel].[FindPerson]
	INSERT INTO #ids SELECT Id FROM Person WHERE IsDeleted = 0
END
ELSE
--Flush and re-load only PersonIds in string
BEGIN
	INSERT INTO #ids SELECT * FROM SplitStringString(@persons)
	DELETE FROM [ReadModel].[FindPerson] WITH(TABLOCK) WHERE PersonId in(SELECT * FROM #ids)
END

--select NEWID()
INSERT [ReadModel].[FindPerson]
SELECT p.Id, p.FirstName, p.LastName, p.EmploymentNumber, p.Note, p.TerminalDate, p.FirstName, 'FirstName', NULL, NULL, NULL,'3474AF5E-4671-4A0F-9E12-0794EF922A47',isnull(pp.StartDate,'19900101'),  pp.EndDate, NULL 
FROM Person p WITH (NOLOCK) 
INNER JOIN #ids ids ON p.Id = ids.person
LEFT JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
WHERE IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT p.Id, p.FirstName, p.LastName, p.EmploymentNumber, p.Note, p.TerminalDate, p.LastName, 'LastName', NULL, NULL, NULL,'4F77EC99-883C-45FD-9CDD-E0CDFE16BCFD',isnull(pp.StartDate,'19900101'),  pp.EndDate, NULL
FROM Person p WITH (NOLOCK) 
INNER JOIN #ids ids ON p.Id = ids.person
LEFT JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
WHERE IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT p.Id, p.FirstName, p.LastName, p.EmploymentNumber, p.Note, p.TerminalDate, p.Note, 'Note', NULL, NULL, NULL,'7D70F7C9-B4EF-4BF9-9BB2-CA2C483F3360', isnull(pp.StartDate,'19900101'),  pp.EndDate, NULL
FROM Person p WITH (NOLOCK) 
INNER JOIN #ids ids ON p.Id = ids.person
LEFT JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
WHERE IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT p.Id, p.FirstName, p.LastName, p.EmploymentNumber, p.Note, p.TerminalDate, p.EmploymentNumber, 'EmploymentNumber', NULL, NULL, NULL, 'ABC2656E-A2E4-46A6-842A-6E731D611D8F', isnull(pp.StartDate,'19900101'),  pp.EndDate, NULL
FROM Person p WITH (NOLOCK) 
INNER JOIN #ids ids ON p.Id = ids.person
LEFT JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
WHERE IsDeleted = 0


INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, ptp.Name, 'PartTimePercentage', NULL, NULL, NULL, ptp.Id, pp.StartDate, pp.EndDate, pp.Team   
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
INNER JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN PartTimePercentage ptp WITH (NOLOCK) ON pp.PartTimePercentage = ptp.Id
WHERE p.IsDeleted = 0 AND ptp.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, rsb.Name, 'ShiftBag', NULL, NULL, NULL, rsb.Id, pp.StartDate, pp.EndDate, pp.Team   
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
INNER JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN RuleSetBag rsb WITH (NOLOCK) ON pp.RuleSetBag = rsb.Id
WHERE p.IsDeleted = 0 AND rsb.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, c.Name, 'Contract', NULL, NULL, NULL, c.Id, pp.StartDate, pp.EndDate, pp.Team   
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
INNER JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN Contract c WITH (NOLOCK) ON pp.Contract = c.Id
WHERE p.IsDeleted = 0 AND c.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, cs.Name, 'ContractSchedule', NULL, NULL, NULL, cs.Id, pp.StartDate, pp.EndDate, pp.Team     
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
INNER JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN ContractSchedule cs WITH (NOLOCK) ON pp.ContractSchedule = cs.Id
WHERE p.IsDeleted = 0 AND cs.IsDeleted = 0 

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, bg.Name, 'BudgetGroup', NULL, NULL, NULL, bg.Id, pp.StartDate, pp.EndDate, pp.Team     
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
INNER JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN BudgetGroup bg WITH (NOLOCK) ON pp.BudgetGroup = bg.Id
WHERE p.IsDeleted = 0 AND bg.IsDeleted = 0 

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, s.Name + ' ' + t.Name, 'Organization', NULL, NULL, NULL, pp.Id, pp.StartDate, pp.EndDate, pp.Team       
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
INNER JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN Team t WITH (NOLOCK) ON pp.Team = t.Id
INNER JOIN Site s WITH (NOLOCK) ON s.Id = t.Site
WHERE p.IsDeleted = 0 AND t.IsDeleted = 0 AND s.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, s.Name, 'Skill', NULL, NULL, NULL, s.Id, pp.StartDate, pp.EndDate, pp.Team    
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
INNER JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN PersonSkill ps WITH (NOLOCK) ON pp.Id = ps.Parent
INNER JOIN Skill s WITH (NOLOCK) ON ps.Skill = s.Id
WHERE p.IsDeleted = 0 AND Active = 1 AND  s.IsDeleted = 0

/*
exec [ReadModel].[UpdateFindPerson] 'B8F5BE96-2C4D-44B5-A5DB-9C6400BB5860'
*/

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id, p.FirstName, p.LastName, p.EmploymentNumber, p.Note, p.TerminalDate, 
CASE SUBSTRING( ar.DescriptionText ,1 , 2 )
WHEN  'xx'    THEN ar.Name
 ELSE ar.DescriptionText
 END
,'Role', NULL, NULL, NULL, ar.Id, ISNULL(pp.StartDate,'19000101'), pp.EndDate, pp.Team  
FROM Person p WITH (NOLOCK)
INNER JOIN #ids ids ON p.Id = ids.person
LEFT JOIN PersonPeriod pp WITH (NOLOCK) ON p.Id = pp.Parent
INNER JOIN PersonInApplicationRole pa WITH (NOLOCK) ON pa.Person = p.Id
INNER JOIN ApplicationRole ar WITH (NOLOCK) ON ar.Id = pa.ApplicationRole
WHERE p.IsDeleted = 0 AND ar.IsDeleted = 0

DECLARE @date DATETIME
SELECT @date = CONVERT(DATETIME, CONVERT(varchar(10), GETDATE(), 101))

INSERT INTO #CurrentPeriod (Parent, Team)
SELECT Parent,Team
FROM [PersonPeriod] pp WITH (NOLOCK)
WHERE pp.StartDate < @date AND pp.EndDate >= @date

UPDATE [ReadModel].[FindPerson] 
SET [TeamId] = t.Id,
	[SiteId] = s.Id,
	[BusinessUnitId] = s.BusinessUnit,
    [PersonPeriodTeamId] = 	ISNULL(p.PersonPeriodTeamId, t.Id)
FROM [ReadModel].[FindPerson] p
LEFT JOIN #CurrentPeriod pp ON p.PersonId = pp.Parent AND pp.Parent in (SELECT person FROM #ids)
INNER JOIN Team t WITH (NOLOCK) ON pp.Team = t.Id
INNER JOIN Site s WITH (NOLOCK) ON s.Id = t.Site 
WHERE t.IsDeleted = 0 AND s.IsDeleted = 0

/*last if terminal*/
INSERT INTO #last
SELECT Parent,Team,EndDate FROM
(
SELECT Parent, Team, EndDate, ROW_NUMBER () OVER (PARTITION BY Parent ORDER BY Parent,enddate DESC) row FROM PersonPeriod WITH (NOLOCK)
) AS LastPersonPeriod WHERE row=1

UPDATE [ReadModel].[FindPerson]
SET [TeamId] = t.Id,
	[SiteId] = s.Id,
	[BusinessUnitId] = s.BusinessUnit 
FROM [ReadModel].[FindPerson] p
LEFT JOIN #last pp ON p.PersonId = pp.Parent
INNER JOIN Team t WITH (NOLOCK) ON pp.Team = t.Id
INNER JOIN Site s WITH (NOLOCK) ON s.Id = t.Site
WHERE p.TeamId is null
AND t.IsDeleted = 0 AND s.IsDeleted = 0

GO

--=================
--Finally, when DBManager applies this SP also execute the SP
--=================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateFindPerson]') AND type in (N'P', N'PC')) 
	EXEC [ReadModel].[UpdateFindPerson] '00000000-0000-0000-0000-000000000000'
GO