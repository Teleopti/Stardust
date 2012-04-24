IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadOrganizationForSelector]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadOrganizationForSelector]
GO


-- =============================================
-- Author:		Ola Håkansson
-- Create date: 2012-01-19
-- Description:	Loads (fast) the people in the organization
-- exec ReadModel.LoadOrganizationForSelector 'Organization', '2012-01-01', '3E369557-1B51-421E-8D1C-9EF300E4D921', 0, 1053
-- exec ReadModel.LoadOrganizationForSelector 'ContractSchedule', '2012-01-26', '3E369557-1B51-421E-8D1C-9EF300E4D921', 0, 1053
-- =============================================
CREATE PROCEDURE [ReadModel].[LoadOrganizationForSelector]
@type nvarchar(200), -- Organization or for example Contract, ContractSchedule
@ondate datetime,
@bu uniqueidentifier,
@users bit,
@culture int
AS
BEGIN
--Create needed temp tables
CREATE TABLE #otherBUpersons(
	[Parent] [uniqueidentifier] NOT NULL
)

CREATE TABLE #result(
	[PersonId] [uniqueidentifier] NOT NULL,
	[TeamId] [uniqueidentifier]  NULL,
	[SiteId] [uniqueidentifier]  NULL,
	[BusinessUnitId] [uniqueidentifier]  NULL,
	[Team] [nvarchar](50) NOT NULL,
	[Site] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	EmploymentNumber [nvarchar](50) NOT NULL
	)

CREATE TABLE #tempPerson(
	[Id] [uniqueidentifier] NOT NULL,
	[ppId] [uniqueidentifier] NOT NULL,
	[Team] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](25) NOT NULL,
	[LastName] [nvarchar](25) NOT NULL,
	[EmploymentNumber] [nvarchar](50) NOT NULL,
	[Contract] [uniqueidentifier] NOT NULL,
	[ContractSchedule] [uniqueidentifier] NOT NULL,
	[PartTimePercentage] [uniqueidentifier] NOT NULL,
	[RuleSetBag] [uniqueidentifier] NULL
)

CREATE TABLE #tempSkill(
	[ppId] [uniqueidentifier] NOT NULL,
	[Skill] [uniqueidentifier] NOT NULL
)

CREATE TABLE #otherResult(
	[PersonId] [uniqueidentifier] NOT NULL,
	[TeamId] [uniqueidentifier]  NULL,
	[SiteId] [uniqueidentifier]  NULL,
	[BusinessUnitId] [uniqueidentifier]  NULL,
	[Node] [nvarchar](1024) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	EmploymentNumber [nvarchar](50) NOT NULL
	)

-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
SET NOCOUNT ON;

-- declares
DECLARE @dynamicSQL nvarchar(4000)
DECLARE @collation nvarchar(50)
	
--Set collation
SELECT @collation = cc.[Collation]
FROM ReadModel.CollationCulture() cc
inner join fn_helpcollations() fn
on cc.[Collation] = fn.name collate database_default
WHERE cc.Culture = @culture

--no match? Go for database_default
SELECT @collation = ISNULL(@collation,'database_default')
--re-init @dynamicSQL
SELECT @dynamicSQL=''

INSERT INTO #tempPerson 
SELECT DISTINCT p.Id, pp.Id ppId, pp.Team, FirstName, LastName, EmploymentNumber, Contract, ContractSchedule, PartTimePercentage, RuleSetBag 
FROM PersonPeriodWithEndDate pp
INNER JOIN Person p 
ON pp.Parent = p.Id AND p.IsDeleted = 0
AND @ondate BETWEEN StartDate AND EndDate AND ISNULL(TerminalDate, '2100-01-01') >= @ondate

INSERT INTO #tempSkill
SELECT DISTINCT pp.Id ppId, ps.Skill
FROM PersonPeriodWithEndDate pp
INNER JOIN PersonSkill ps
ON pp.Id = ps.Parent
AND @ondate BETWEEN StartDate AND EndDate

IF @type = 'Organization' BEGIN
		
	INSERT #result	
	SELECT tp.Id, Team.Id, Site.Id, BusinessUnit, Team.Name, Site.Name, FirstName, LastName, EmploymentNumber
	FROM #tempPerson tp
	INNER JOIN Team ON Team.Id = tp.Team AND Team.IsDeleted = 0  
	INNER JOIN Site ON Site.id = Site AND Site.IsDeleted = 0 AND BusinessUnit = @bu
	WHERE BusinessUnit = @bu
	
	IF @users = 1 BEGIN 
		SELECT DISTINCT Parent INTO #otherBU FROM PersonPeriod pp INNER JOIN Team ON Team.Id = pp.Team
		INNER JOIN Site ON Site.id = Site  WHERE BusinessUnit <> @bu
		
		INSERT #result
		SELECT Id, null, null, null, '', '',  FirstName, LastName, EmploymentNumber
		FROM  Person 
		WHERE Id NOT IN (SELECT PersonId FROM #result)
		AND Id NOT IN (SELECT Id FROM #otherBU)
		AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
		AND IsDeleted = 0 AND BuiltIn = 0

	END
	
	
	SELECT @dynamicSQL =	'SELECT [PersonId], [TeamId], [SiteId], [BusinessUnitId], [Team], [Site], [FirstName], [LastName],	EmploymentNumber FROM #result ORDER BY Site collate ' + @collation +
							', Team collate ' + @collation +
							', LastName collate ' + @collation +
							', FirstName collate ' + @collation 
		
		EXEC sp_executesql @dynamicSQL 
		
	RETURN
				
	END

	IF @type = 'Contract' BEGIN
	
	INSERT #otherResult
	SELECT DISTINCT tp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name, 
	FirstName, LastName, EmploymentNumber  
	FROM #tempPerson tp
	INNER JOIN Team ON Team.Id = tp.Team
	INNER JOIN Site ON Site.id = Team.Site and Site.BusinessUnit = @bu
	INNER JOIN Contract c ON tp.Contract = c.Id AND c.IsDeleted = 0

	END
	
	IF @type = 'ContractSchedule' BEGIN
	
	INSERT #otherResult
	SELECT DISTINCT tp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name, 
	FirstName, LastName, EmploymentNumber  
	FROM #tempPerson tp
	INNER JOIN Team ON Team.Id = tp.Team
	INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
	INNER JOIN ContractSchedule c ON tp.ContractSchedule = c.Id AND c.IsDeleted = 0
	
	END

IF @type = 'PartTimePercentage' BEGIN
	
	INSERT #otherResult
	SELECT DISTINCT tp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name,
	FirstName, LastName, EmploymentNumber   
	FROM #tempPerson tp
	INNER JOIN Team ON Team.Id = tp.Team
	INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
	INNER JOIN PartTimePercentage c ON tp.PartTimePercentage = c.Id AND c.IsDeleted = 0
 
	END
	
IF @type = 'Note' BEGIN

	INSERT INTO #otherBUpersons
	SELECT DISTINCT Parent
	 FROM PersonPeriod pp INNER JOIN Team ON Team.Id = pp.Team
	INNER JOIN Site ON Site.id = Site  WHERE BusinessUnit <> @bu
		
	INSERT #otherResult
	SELECT DISTINCT p.Id, null, null, null ,Note,  
	FirstName, LastName, EmploymentNumber  
	FROM Person p
	WHERE ISNULL(TerminalDate, '2100-01-01') >= @ondate
	AND Id NOT IN (SELECT * FROM #otherBUpersons) -- om folk byter BU blir detta problem
	AND p.IsDeleted = 0
	AND Note <> ''
	 
	END
	
	IF @type = 'ShiftBag' BEGIN
	
	INSERT #otherResult
	SELECT DISTINCT tp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name,  
	FirstName, LastName, EmploymentNumber  
	FROM #tempPerson tp
	INNER JOIN Team ON Team.Id = tp.Team
	INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
	INNER JOIN RuleSetBag c ON tp.RuleSetBag = c.Id AND c.IsDeleted = 0 
	 
	END
	
	IF @type = 'Skill' BEGIN
	INSERT #otherResult
	SELECT DISTINCT tp.Id, Team.Id, Site.Id, Site.BusinessUnit ,s.Name,  
	FirstName, LastName, EmploymentNumber  
	FROM #tempPerson tp
	INNER JOIN Team ON Team.Id = tp.Team
	INNER JOIN Site ON Site.id = Team.Site and Site.BusinessUnit = @bu
	INNER JOIN #tempSkill ps ON tp.ppId = ps.ppId
	INNER JOIN Skill s ON ps.Skill = s.Id AND s.IsDeleted = 0
	 
	END
	
	SELECT @dynamicSQL =	'SELECT [PersonId], [TeamId], [SiteId], [BusinessUnitId], [Node], [FirstName], [LastName],	EmploymentNumber FROM #otherResult ORDER BY Node collate ' + @collation +
							', LastName collate ' + @collation +
							', FirstName collate ' + @collation 
		
	EXEC sp_executesql @dynamicSQL 
END




GO


