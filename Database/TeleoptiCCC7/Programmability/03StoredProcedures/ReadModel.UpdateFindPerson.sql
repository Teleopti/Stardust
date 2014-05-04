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
-- exec [ReadModel].[UpdateFindPerson] 'B0C67CB1-1C4F-4047-8DC1-9EF500DC79A6, 2AE730A0-5AF7-49B7-9498-9EF500DC79A6'
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

CREATE TABLE #BuiltinIds(
	PersonId uniqueidentifier NOT NULL
)

INSERT INTO #BuiltinIds SELECT Id FROM Person WHERE BuiltIn = 1

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
	DELETE FROM [ReadModel].[FindPerson] WHERE PersonId in(SELECT * FROM #ids)
END

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, FirstName, 'FirstName', NULL, NULL, NULL,NULL
FROM Person WHERE IsDeleted = 0
AND Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'FirstName')

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, LastName, 'LastName', NULL, NULL, NULL,NULL  
FROM Person WHERE IsDeleted = 0
AND Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'LastName')

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, Note, 'Note', NULL, NULL, NULL,NULL 
FROM Person WHERE IsDeleted = 0
AND Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'Note')

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, EmploymentNumber, 'EmploymentNumber', NULL, NULL, NULL,NULL  
FROM Person WHERE IsDeleted = 0
AND Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'EmploymentNumber')

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, [Identity] as WindowsLogOnName, 'WindowsLogOnName', NULL, NULL, NULL, NULL  
FROM Person INNER JOIN AuthenticationInfo On Id = Person AND IsDeleted = 0
AND Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'WindowsLogOnName')

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, ApplicationLogOnName, 'ApplicationLogOnName', NULL, NULL, NULL,NULL  
FROM Person INNER JOIN ApplicationAuthenticationInfo On Id = Person AND IsDeleted = 0
AND Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'ApplicationLogOnName')

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, ptp.Name, 'PartTimePercentage', NULL, NULL, NULL, ptp.Id
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN PartTimePercentage ptp ON pp.PartTimePercentage = ptp.Id
WHERE p.IsDeleted = 0 AND ptp.IsDeleted = 0
AND p.Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'PartTimePercentage')

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, rsb.Name, 'ShiftBag', NULL, NULL, NULL, rsb.Id
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN RuleSetBag rsb ON pp.RuleSetBag = rsb.Id
WHERE p.IsDeleted = 0 AND rsb.IsDeleted = 0
AND p.Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'ShiftBag')

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, c.Name, 'Contract', NULL, NULL, NULL, c.Id 
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN Contract c ON pp.Contract = c.Id
WHERE p.IsDeleted = 0 AND c.IsDeleted = 0
AND p.Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'Contract')

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, cs.Name, 'ContractSchedule', NULL, NULL, NULL, cs.Id  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN ContractSchedule cs ON pp.ContractSchedule = cs.Id
WHERE p.IsDeleted = 0 AND cs.IsDeleted = 0 
AND p.Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'ContractSchedule')

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, bg.Name, 'BudgetGroup', NULL, NULL, NULL, bg.Id
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN BudgetGroup bg ON pp.BudgetGroup = bg.Id
WHERE p.IsDeleted = 0 AND bg.IsDeleted = 0 
AND p.Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'BudgetGroup')

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, t.Name + ' ' + s.Name, 'Organization', NULL, NULL, NULL, pp.Id  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN Site s ON s.Id = t.Site
WHERE p.IsDeleted = 0 AND t.IsDeleted = 0 AND s.IsDeleted = 0
AND p.Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'Organization')

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, s.Name, 'Skill', NULL, NULL, NULL, s.Id  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN PersonSkill ps ON pp.Id = ps.Parent
INNER JOIN Skill s ON ps.Skill = s.Id
WHERE p.IsDeleted = 0 AND Active = 1 AND  s.IsDeleted = 0
AND p.Id NOT IN(SELECT PersonId FROM [ReadModel].[FindPerson] WHERE SearchType = 'Skill')

DECLARE @date DATETIME
SELECT @date = CONVERT(DATETIME, CONVERT(varchar(10), GETDATE(), 101))

INSERT INTO #CurrentPeriod (Parent, Team)
SELECT Parent,Team
FROM [PersonPeriodWithEndDate] pp
WHERE pp.StartDate < @date AND pp.EndDate >= @date

UPDATE [ReadModel].[FindPerson] SET [TeamId] = t.Id,
	[SiteId] = s.Id,[BusinessUnitId] = s.BusinessUnit 
FROM [ReadModel].[FindPerson] p
LEFT JOIN #CurrentPeriod pp ON p.PersonId = pp.Parent AND pp.Parent in (SELECT * FROM #ids)
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN Site s ON s.Id = t.Site
-- the 
WHERE t.IsDeleted = 0 AND s.IsDeleted = 0

/*last if terminal*/
INSERT INTO #last
SELECT Parent,Team,EndDate FROM
(
SELECT Parent, Team, EndDate, ROW_NUMBER () OVER (PARTITION BY Parent ORDER BY Parent,enddate DESC) row FROM PersonPeriodWithEndDate
) AS LastPersonPeriod WHERE row=1

UPDATE [ReadModel].[FindPerson] SET [TeamId] = t.Id,
	[SiteId] = s.Id,[BusinessUnitId] = s.BusinessUnit 
FROM [ReadModel].[FindPerson] p
LEFT JOIN #last pp ON p.PersonId = pp.Parent
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN Site s ON s.Id = t.Site
WHERE p.TeamId is null
AND t.IsDeleted = 0 AND s.IsDeleted = 0

delete fp
from #BuiltinIds bIds
inner join [ReadModel].[FindPerson] fp
            on fp.PersonId = bIds.PersonId

GO

--=================
--Finally, when DBManager applies this SP also execute the SP
--=================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateFindPerson]') AND type in (N'P', N'PC')) 
EXEC [ReadModel].[UpdateFindPerson] '00000000-0000-0000-0000-000000000000'
GO