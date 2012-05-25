IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadOrganizationForSelector]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadOrganizationForSelector]
GO

-- =============================================
-- Author:		Ola Håkansson
-- Create date: 2012-01-19
-- Description:	Loads (fast) the people in the organization
-- exec ReadModel.LoadOrganizationForSelector 'Organization', '2012-01-01', '3E369557-1B51-421E-8D1C-9EF300E4D921', 0, 1053
-- exec ReadModel.LoadOrganizationForSelector 'ContractSchedule', '2012-01-26', '3E369557-1B51-421E-8D1C-9EF300E4D921', 0, 1053
-- ======change log=======
-- When			Who		Why
-- 2012-03-13	DavidJ	Fix Azure compability => Remove unwanted SET commands, No SELECT INTO
-- 2012-04-25	DavidJ	Manual merg to 356

-- =============================================
CREATE PROCEDURE [ReadModel].[LoadOrganizationForSelector]
@type nvarchar(200), -- Organization or for example Contract, ContractSchedule
@ondate datetime,
@enddate datetime,
@bu uniqueidentifier,
@users bit,
@culture int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--Create all temp tables first, for performance
	CREATE TABLE #otherBU(
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

	CREATE TABLE #otherBUpersons(
		[Parent] [uniqueidentifier] NOT NULL
	)

	--declare
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

	IF @type = 'Organization'
	BEGIN
		INSERT #result
		SELECT Person.Id, Team.Id, Site.Id, BusinessUnit, Team.Name, Site.Name, FirstName, LastName, EmploymentNumber
		FROM PersonPeriodWithEndDate pp
		INNER JOIN Person ON pp.Parent = Person.Id AND Person.IsDeleted = 0 AND StartDate <= @enddate AND EndDate >= @ondate
	AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
		INNER JOIN Team ON Team.Id = pp.Team AND Team.IsDeleted = 0  
		INNER JOIN Site ON Site.id = Site AND Site.IsDeleted = 0 AND BusinessUnit = @bu
		
		IF @users = 1
		BEGIN
			INSERT INTO #otherBU
			SELECT DISTINCT Parent FROM PersonPeriod pp INNER JOIN Team ON Team.Id = pp.Team
			INNER JOIN Site ON Site.id = Site  WHERE BusinessUnit <> @bu
			
			INSERT #result
			SELECT Id, null, null, null, '', '',  FirstName, LastName, EmploymentNumber
			FROM  Person 
			WHERE Id NOT IN (SELECT PersonId FROM #result)
			AND Id NOT IN (SELECT * FROM #otherBU)
		AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
			AND IsDeleted = 0 AND BuiltIn = 0

		END
		
		
		SELECT @dynamicSQL =	'SELECT * FROM #result ORDER BY Site collate ' + @collation +
								', Team collate ' + @collation +
								', LastName collate ' + @collation +
								', FirstName collate ' + @collation 
			
		EXEC sp_executesql @dynamicSQL 
			
		RETURN
	END
		
			
	IF @type = 'Contract'
	BEGIN
		INSERT #otherResult
		SELECT DISTINCT p.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name, 
		FirstName, LastName, EmploymentNumber  
		FROM Person p
		INNER JOIN PersonPeriodWithEndDate pp ON p.Id = pp.Parent AND p.IsDeleted = 0 AND StartDate <= @enddate AND EndDate >= @ondate
	AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
		INNER JOIN Team ON Team.Id = pp.Team
		INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN Contract c ON pp.Contract = c.Id AND c.IsDeleted = 0
	END
		
	IF @type = 'ContractSchedule'
	BEGIN

		INSERT #otherResult
		SELECT DISTINCT p.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name, 
		FirstName, LastName, EmploymentNumber  
		FROM Person p
		INNER JOIN PersonPeriodWithEndDate pp ON p.Id = pp.Parent AND p.IsDeleted = 0 AND StartDate <= @enddate AND EndDate >= @ondate
	AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
		INNER JOIN Team ON Team.Id = pp.Team
		INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN ContractSchedule c ON pp.ContractSchedule = c.Id AND c.IsDeleted = 0

	END

	IF @type = 'PartTimePercentage'
	BEGIN
		INSERT #otherResult
		SELECT DISTINCT p.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name,
		FirstName, LastName, EmploymentNumber   
		FROM Person p
		INNER JOIN PersonPeriodWithEndDate pp ON p.Id = pp.Parent AND p.IsDeleted = 0 AND StartDate <= @enddate AND EndDate >= @ondate
	AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
		INNER JOIN Team ON Team.Id = pp.Team
		INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN PartTimePercentage c ON pp.PartTimePercentage = c.Id AND c.IsDeleted = 0
	END
		
	IF @type = 'Note'
	BEGIN
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
		
	IF @type = 'ShiftBag'
	BEGIN
		
		INSERT #otherResult
		SELECT DISTINCT p.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name,  
		FirstName, LastName, EmploymentNumber  
		FROM Person p 
		INNER JOIN PersonPeriodWithEndDate pp ON p.Id = pp.Parent AND p.IsDeleted = 0 AND StartDate <= @enddate AND EndDate >= @ondate
	AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
		INNER JOIN Team ON Team.Id = pp.Team
		INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN RuleSetBag c ON pp.RuleSetBag = c.Id AND c.IsDeleted = 0 
	END
		
	IF @type = 'Skill'
	BEGIN

		INSERT #otherResult
		SELECT DISTINCT p.Id, Team.Id, Site.Id, Site.BusinessUnit ,s.Name,  
		FirstName, LastName, EmploymentNumber  
		FROM Person p
		INNER JOIN PersonPeriodWithEndDate pp ON p.Id = pp.Parent AND p.IsDeleted = 0 AND StartDate <= @enddate AND EndDate >= @ondate
	AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
		INNER JOIN Team ON Team.Id = pp.Team
		INNER JOIN Site ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN PersonSkill ps ON pp.Id = ps.Parent
		INNER JOIN Skill s ON ps.Skill = s.Id AND s.IsDeleted = 0
	 
	END

	SELECT @dynamicSQL =	'SELECT * FROM #otherResult ORDER BY Node collate ' + @collation +
							', LastName collate ' + @collation +
							', FirstName collate ' + @collation 
		
	EXEC sp_executesql @dynamicSQL 
END
GO


