IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[LoadOrganizationForSelector_Web]') AND type in (N'P', N'PC'))
DROP PROCEDURE [ReadModel].[LoadOrganizationForSelector_Web]
GO
-- =============================================
-- Author:		Dumpling
-- Create date: 2017-06-12
-- Description:	Loads (fast) the teams and sites in the organization on web
-- =============================================
CREATE PROCEDURE [ReadModel].[LoadOrganizationForSelector_Web]
@ondate datetime,
@enddate datetime,
@bu uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	CREATE TABLE #result(
		[TeamId] [uniqueidentifier]  NULL,
		[SiteId] [uniqueidentifier]  NULL,
		[Team] [nvarchar](50) NOT NULL,
		[Site] [nvarchar](50) NOT NULL
		)

	
	CREATE TABLE #TempPersonPeriodWithEndDate (
		PersonId uniqueidentifier,
		Team uniqueidentifier
	)

    CREATE TABLE #TempDeletedPerson (
		id uniqueidentifier
	)

	CREATE TABLE #TempDeletedTeam (
		id uniqueidentifier
	)
	
	CREATE TABLE #TempTeamWithPerson (
		id uniqueidentifier
	)
		
	CREATE TABLE #TempPersonWithTeam (
		id uniqueidentifier
	)

	INSERT INTO #TempDeletedTeam
	SELECT  Team.Id from Team with(nolock)
	INNER JOIN Site with(nolock) ON Site.id = Team.Site and Site.BusinessUnit =@bu
	where (Team.IsDeleted = 1 or Site.IsDeleted = 1) 


	
	INSERT INTO #TempDeletedPerson
	SELECT Id from Person with(nolock) where  Person.IsDeleted = 1
	or ISNULL(TerminalDate, '2100-01-01') < @ondate 

	INSERT INTO #TempPersonPeriodWithEndDate 
		SELECT pp.Parent, Team
		FROM PersonPeriod pp with(nolock)
		Where StartDate <= @enddate AND EndDate >= @ondate

	INSERT INTO #TempTeamWithPerson
		SELECT distinct(Team)
		FROM #TempPersonPeriodWithEndDate sub
		where not exists (select id from #TempDeletedTeam dt where dt.id = sub.Team) 
	    and not exists (select id from #TempDeletedPerson dt where dt.id = sub.PersonId )
	

	--declare
	DECLARE @dynamicSQL nvarchar(4000)

	--re-init @dynamicSQL
	SELECT @dynamicSQL=''
	
		INSERT #result
		SELECT sub.id, Team.Site, team.Name, Site.Name
		from #TempTeamWithPerson sub
		inner join Team with(nolock) on sub.id = Team.Id
		inner join Site with(nolock) on Team.Site = Site.Id and Site.BusinessUnit =@bu
		
		SELECT @dynamicSQL = 'SELECT * FROM #result'
		EXEC sp_executesql @dynamicSQL 
			
	RETURN
END
GO
