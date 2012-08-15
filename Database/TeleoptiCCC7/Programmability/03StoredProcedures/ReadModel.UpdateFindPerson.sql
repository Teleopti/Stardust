
/****** Object:  StoredProcedure [ReadModel].[UpdateFindPerson]    Script Date: 12/19/2011 14:51:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateFindPerson]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[UpdateFindPerson]
GO

/****** Object:  StoredProcedure [ReadModel].[UpdateFindPerson]    Script Date: 12/19/2011 14:51:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [ReadModel].[UpdateFindPerson]
@persons nvarchar(max)

-- =============================================
-- Author:		Ola
-- Create date: 2011-12-19
-- Description:	Updates the read model for finding persons
-- Change:		Added a parameter which persons we should update

-- =============================================
AS
-- exec [ReadModel].[UpdateFindPerson] 'B0C67CB1-1C4F-4047-8DC1-9EF500DC79A6, 2AE730A0-5AF7-49B7-9498-9EF500DC79A6'
 SET NOCOUNT ON
CREATE TABLE #ids(person uniqueidentifier)
INSERT INTO #ids SELECT * FROM SplitStringString(@persons) 

DELETE FROM [ReadModel].[FindPerson] WHERE PersonId in(SELECT * FROM #ids)

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
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, WindowsLogOnName, 'WindowsLogOnName', NULL, NULL, NULL, NULL  
FROM Person INNER JOIN WindowsAuthenticationInfo On Id = Person AND IsDeleted = 0
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

UPDATE [ReadModel].[FindPerson] SET [TeamId] = t.Id,
	[SiteId] = s.Id,[BusinessUnitId] = s.BusinessUnit 
FROM [ReadModel].[FindPerson] p
LEFT JOIN [PersonPeriodWithEndDate] pp ON p.PersonId = pp.Parent and pp.Parent in (SELECT * FROM #ids)
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN Site s ON s.Id = t.Site
-- the 
WHERE pp.StartDate < GETDATE() AND pp.EndDate > GETDATE()
AND t.IsDeleted = 0 AND s.IsDeleted = 0


