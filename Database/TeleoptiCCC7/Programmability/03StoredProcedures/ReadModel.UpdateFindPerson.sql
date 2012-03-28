
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


-- =============================================
-- Author:		Ola
-- Create date: 2011-12-19
-- Description:	Updates the read model for finding persons
-- Change:		
-- =============================================
AS
-- exec [ReadModel].[UpdateFindPerson]
SET NOCOUNT ON
DELETE FROM [ReadModel].[FindPerson]

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, FirstName, '', NULL, NULL, NULL 
FROM Person WHERE IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, LastName, '', NULL, NULL, NULL  
FROM Person WHERE IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, Note, '', NULL, NULL, NULL 
FROM Person WHERE IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, EmploymentNumber, '', NULL, NULL, NULL  
FROM Person WHERE IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, WindowsLogOnName, '', NULL, NULL, NULL  
FROM Person INNER JOIN WindowsAuthenticationInfo On Id = Person AND IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT Id,FirstName, LastName, EmploymentNumber, Note, TerminalDate, ApplicationLogOnName, '', NULL, NULL, NULL  
FROM Person INNER JOIN ApplicationAuthenticationInfo On Id = Person AND IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, ptp.Name, 'PartTimePercentage', NULL, NULL, NULL  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN PartTimePercentage ptp ON pp.PartTimePercentage = ptp.Id
WHERE p.IsDeleted = 0 AND ptp.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, rsb.Name, 'ShiftBag', NULL, NULL, NULL  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN RuleSetBag rsb ON pp.RuleSetBag = rsb.Id
WHERE p.IsDeleted = 0 AND rsb.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, c.Name, 'Contract', NULL, NULL, NULL  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN Contract c ON pp.Contract = c.Id
WHERE p.IsDeleted = 0 AND c.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, cs.Name, 'ContractSchedule', NULL, NULL, NULL  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN ContractSchedule cs ON pp.ContractSchedule = cs.Id
WHERE p.IsDeleted = 0 AND cs.IsDeleted = 0 

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, bg.Name, 'BudgetGroup', NULL, NULL, NULL  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN BudgetGroup bg ON pp.BudgetGroup = bg.Id
WHERE p.IsDeleted = 0 AND bg.IsDeleted = 0 

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, t.Name + ' ' + s.Name, 'Organization', NULL, NULL, NULL  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN Site s ON s.Id = t.Site
WHERE p.IsDeleted = 0 AND t.IsDeleted = 0 AND s.IsDeleted = 0

INSERT [ReadModel].[FindPerson]
SELECT DISTINCT p.Id,FirstName, LastName, EmploymentNumber, p.Note, TerminalDate, s.Name, 'Skill', NULL, NULL, NULL  
FROM Person p
INNER JOIN PersonPeriod pp ON p.Id = pp.Parent
INNER JOIN PersonSkill ps ON pp.Id = ps.Parent
INNER JOIN Skill s ON ps.Skill = s.Id
WHERE p.IsDeleted = 0 AND Active = 1 AND  s.IsDeleted = 0


UPDATE [ReadModel].[FindPerson] SET [TeamId] = t.Id,
	[SiteId] = s.Id,[BusinessUnitId] = s.BusinessUnit 
FROM [ReadModel].[FindPerson] p
LEFT JOIN [PersonPeriodWithEndDate] pp ON p.PersonId = pp.Parent
INNER JOIN Team t ON pp.Team = t.Id
INNER JOIN Site s ON s.Id = t.Site
-- the 
WHERE pp.StartDate < GETDATE() AND pp.EndDate > GETDATE()
AND t.IsDeleted = 0 AND s.IsDeleted = 0

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[UpdateFindPerson]') AND type in (N'P', N'PC')) 
AND (SELECT COUNT(*) FROM [ReadModel].[FindPerson]) = 0
exec [ReadModel].[UpdateFindPerson]
GO