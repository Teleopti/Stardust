IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadOrganizationForSelector]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadOrganizationForSelector]
GO
-- =============================================
-- Author:		Ola Håkansson
-- Create date: 2012-01-19
-- Description:	Loads (fast) the people in the organization
-- exec ReadModel.LoadOrganizationForSelector 'Organization', '2012-01-01', '3E369557-1B51-421E-8D1C-9EF300E4D921', 0, 1053
-- exec ReadModel.LoadOrganizationForSelector 'Note', '2012-01-26', '2012-01-26' ,'4601AD46-D82C-4190-9540-9E2101028E3A', 0, 1053
-- ======change log=======
-- When			Who		Why
-- 2012-03-13	DavidJ	Fix Azure compability => Remove unwanted SET commands, No SELECT INTO
-- 2012-09-03	Ola		Use the #TempPersonPeriodWithEndDate everywhere for speed
-- 2012-10-08	Ola		Bug in Notes when we did not fetch the teams and sites 
-- =============================================
CREATE PROCEDURE [ReadModel].[LoadOrganizationForSelector]
@type nvarchar(200), -- Organization or for example Contract, ContractSchedule
@ondate datetime,
@enddate datetime,
@bu uniqueidentifier,
@users bit,
@culture int,
@optionalColumnId uniqueidentifier
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
	
	CREATE TABLE #TempPersonPeriodWithEndDate (
		id uniqueidentifier,
		FirstName [nvarchar](50),
		LastName [nvarchar](50),
		EmploymentNumber [nvarchar](50),
		Team [nvarchar](50),
		Parent [uniqueidentifier],
		[Contract]  [uniqueidentifier],
		[ContractSchedule]  [uniqueidentifier],
		PartTimePercentage  [uniqueidentifier],
		RuleSetBag [uniqueidentifier],
		PeriodId [uniqueidentifier],
		Note [nvarchar](1024)
	)
	
	INSERT INTO #TempPersonPeriodWithEndDate 
		SELECT Person.Id, FirstName, LastName, EmploymentNumber,Team, Parent, [Contract], [ContractSchedule], 
		PartTimePercentage, RuleSetBag, pp.Id, pp.Note
		FROM PersonPeriod pp with (nolock)
		INNER JOIN Person with (nolock) ON pp.Parent = Person.Id AND Person.IsDeleted = 0 AND StartDate <= @enddate AND EndDate >= @ondate
		and ISNULL(TerminalDate, '2100-01-01') >= @ondate

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
		SELECT sub.Id, Team.Id, Site.Id, BusinessUnit, Team.Name, Site.Name, FirstName, LastName, EmploymentNumber
	from #TempPersonPeriodWithEndDate sub
		INNER JOIN Team with (nolock) ON Team.Id = sub.Team AND Team.IsDeleted = 0  
		INNER JOIN Site with (nolock) ON Site.id = Site AND Site.IsDeleted = 0 AND BusinessUnit = @bu	
		
		IF @users = 1
		BEGIN
			INSERT INTO #otherBU
			SELECT DISTINCT Parent FROM PersonPeriod pp with (nolock) INNER JOIN Team with (nolock) ON Team.Id = pp.Team
			INNER JOIN Site with (nolock) ON Site.id = Site  WHERE BusinessUnit <> @bu
			
			INSERT #result
			SELECT Id, null, null, null, '', '',  FirstName, LastName, EmploymentNumber
			FROM  Person 
			WHERE Id NOT IN (SELECT PersonId FROM #result)
			AND Id NOT IN (SELECT * FROM #otherBU)
		AND ISNULL(TerminalDate, '2100-01-01') >= @ondate
			AND IsDeleted = 0

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
		SELECT DISTINCT pp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name, 
		FirstName, LastName, EmploymentNumber  
		FROM  #TempPersonPeriodWithEndDate pp 
		INNER JOIN Team with (nolock) ON Team.Id = pp.Team
		INNER JOIN Site with (nolock) ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN Contract c with (nolock) ON pp.Contract = c.Id AND c.IsDeleted = 0
	END
		
	IF @type = 'ContractSchedule'
	BEGIN

		INSERT #otherResult
		SELECT DISTINCT pp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name, 
		FirstName, LastName, EmploymentNumber  
		FROM #TempPersonPeriodWithEndDate pp 
		INNER JOIN Team with (nolock) ON Team.Id = pp.Team
		INNER JOIN Site with (nolock) ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN ContractSchedule c with (nolock) ON pp.ContractSchedule = c.Id AND c.IsDeleted = 0

	END

	IF @type = 'PartTimePercentage'
	BEGIN
		INSERT #otherResult
		SELECT DISTINCT pp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name,
		FirstName, LastName, EmploymentNumber   
		FROM #TempPersonPeriodWithEndDate pp
		INNER JOIN Team with (nolock) ON Team.Id = pp.Team
		INNER JOIN Site with (nolock) ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN PartTimePercentage c with (nolock) ON pp.PartTimePercentage = c.Id AND c.IsDeleted = 0
	END
		
	IF @type = 'Note'
	BEGIN	
		INSERT #otherResult
		SELECT DISTINCT pp.Id, Team.Id, Site.Id, Site.BusinessUnit , p.Note,  
		pp.FirstName, pp.LastName, pp.EmploymentNumber  
		FROM  #TempPersonPeriodWithEndDate pp
		INNER JOIN Person p with (nolock) on pp.Parent = p.Id
		INNER JOIN Team with (nolock) ON Team.Id = pp.Team
		INNER JOIN Site with (nolock) ON Site.id = Site and Site.BusinessUnit = @bu
		AND p.Note <> ''
	END
		
	IF @type = 'ShiftBag'
	BEGIN
		
		INSERT #otherResult
		SELECT DISTINCT pp.Id, Team.Id, Site.Id, Site.BusinessUnit ,c.Name,  
		FirstName, LastName, EmploymentNumber  
		FROM #TempPersonPeriodWithEndDate pp
		INNER JOIN Team with (nolock) ON Team.Id = pp.Team
		INNER JOIN Site with (nolock) ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN RuleSetBag c with (nolock) ON pp.RuleSetBag = c.Id AND c.IsDeleted = 0 
	END
		
	IF @type = 'Skill'
	BEGIN

		INSERT #otherResult
		SELECT DISTINCT pp.Id, Team.Id, Site.Id, Site.BusinessUnit ,s.Name,  
		FirstName, LastName, EmploymentNumber  
		FROM #TempPersonPeriodWithEndDate pp 
		INNER JOIN Team with (nolock) ON Team.Id = pp.Team
		INNER JOIN Site with (nolock) ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN PersonSkill ps with (nolock) ON PeriodId = ps.Parent
		INNER JOIN Skill s with (nolock) ON ps.Skill = s.Id AND s.IsDeleted = 0
	 
	END

	IF @type = 'OptionalColumn'
	BEGIN	
		INSERT #otherResult
		SELECT DISTINCT pp.Id, Team.Id, Site.Id, Site.BusinessUnit, ocv.Description,  
		pp.FirstName, pp.LastName, pp.EmploymentNumber  
		FROM  #TempPersonPeriodWithEndDate pp
		INNER JOIN Person p with (nolock) on pp.Parent = p.Id
		INNER JOIN Team with (nolock) ON Team.Id = pp.Team
		INNER JOIN Site with (nolock) ON Site.id = Site and Site.BusinessUnit = @bu
		INNER JOIN OptionalColumnValue ocv with (nolock) ON p.Id = ocv.ReferenceId 
			AND ocv.Parent = @optionalColumnId
			AND ocv.Description <> ''
	END

	SELECT @dynamicSQL =	'SELECT * FROM #otherResult ORDER BY Node collate ' + @collation +
							', LastName collate ' + @collation +
							', FirstName collate ' + @collation 
		
	EXEC sp_executesql @dynamicSQL 
END
GO


