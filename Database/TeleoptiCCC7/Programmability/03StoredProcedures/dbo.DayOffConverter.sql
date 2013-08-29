IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DayOffConverter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DayOffConverter]
GO
-- =============================================
-- Author:		DJ
-- Create date: 2013-08-26
-- Description:	Called by Security.exe to convert DayOff to new ScheduleDay design
-- Change:		
-- =============================================
CREATE PROCEDURE [dbo].[DayOffConverter]
AS
SET NOCOUNT ON
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PersonDayOff]') AND type in (N'U'))
BEGIN


--debug
--reset DayOff
/* exec DayOffConverter
select * from PersonDayOff
DELETE FROM personAssignment WHERE ShiftCategory IS null
UPDATE personAssignment SET DayOffTemplate = null WHERE DayOffTemplate IS not null

*/

	--temp table used
	CREATE TABLE #personTeam(Person uniqueidentifier,Team uniqueidentifier,StartDate DateTime, OrderIndex int)
	CREATE CLUSTERED INDEX IX_Person ON #personTeam	([Person])

	CREATE TABLE #LastKnownBusinessUnit(Person uniqueidentifier,Businessunit uniqueidentifier)
	CREATE NONCLUSTERED INDEX IX_Person_Businessunit
	ON #LastKnownBusinessUnit ([Person]) INCLUDE ([BusinessUnit])

	CREATE TABLE #DayOffTemplateSameName(Id uniqueidentifier, CreatedOn datetime, Name nvarchar(50), BusinessUnit uniqueidentifier, ValidToDate datetime)
	--declare
	DECLARE @Assert int
	DECLARE @PersonsMultipleBu int
	DECLARE @PersonsWithoutBu int
	DECLARE @superUser uniqueidentifier
	DECLARE @Converted int
	DECLARE @ThisStep int

	--init
	SET @Assert=0
	SET @PersonsMultipleBu=0
	SET @PersonsWithoutBu=0
	SET @superUser='3f0886ab-7b25-4e95-856a-0d726edc2a67'
	SET @Converted=0
	SET @ThisStep=0

	--Reset any previous conversion of DayOff
	UPDATE dbo.PersonAssignment
	SET DayOffTemplate = NULL
	WHERE DayOffTemplate IS NOT NULL

	DELETE dbo.PersonAssignment
	WHERE ShiftCategory IS NULL

	--get the number of dayOff to convert
	select @Assert = count(person) from (
		select distinct Person, Scenario,convert(datetime,floor(convert(decimal(18,8),anchor))) as 'Date'
		from dbo.PersonDayOff
		group by Person, Scenario,convert(datetime,floor(convert(decimal(18,8),anchor)))
	) a
	PRINT '----'
	PRINT 'Total number of DayOff to be converted: ' + +cast(@Assert as nvarchar(10))
	PRINT '----'

	--Check which BU a person belongs to
	;WITH personTeam AS
	(
	select
		pp.Parent as 'Person',
		pp.Team,
		pp.StartDate,
		ROW_NUMBER() OVER (PARTITION BY pp.Parent ORDER BY pp.StartDate DESC) AS rn
	from dbo.Person p
	inner join dbo.PersonPeriod pp
		on pp.Parent = p.Id
	)
	insert into #personTeam(Person,Team,StartDate,OrderIndex)
	select * from personTeam

	--Get Last known BU for all agents
	INSERT INTO #LastKnownBusinessUnit(Person,Businessunit)
	SELECT
		pt.Person,
		bu.Id
	FROM #personTeam pt
	inner join dbo.team t
		on t.Id = pt.Team
	inner join dbo.Site s
		on s.Id = t.Site
	inner join dbo.BusinessUnit bu
		on bu.Id = s.BusinessUnit
	where pt.OrderIndex = 1

	--check number of agents that have belonged to more than one Business Unit
		SELECT
			@PersonsMultipleBu=count(*)
		FROM #personTeam plkt
		inner join dbo.team t
			on t.Id = plkt.Team
		inner join dbo.Site s
			on s.Id = t.Site
		inner join dbo.BusinessUnit bu
			on bu.Id = s.BusinessUnit
		GROUP by plkt.Person
		having count(Bu.id) > 1
	IF @PersonsMultipleBu>0
	BEGIN
		PRINT char(13)
		PRINT '----'
		PRINT char(9) + 'Warning - Some agents have belonged to more than one business unit!!'
		PRINT char(9) + '@PersonsMultipleBu=' +cast(@PersonsMultipleBu as nvarchar(10))
		PRINT char(9) + 'DayOff conversion might put the wrong scenario_id in personAssignment'
		PRINT '----'
	END

	--DayOff connected to a person without Person Period = No Business Unit
	select @PersonsWithoutBu=
	(
		select count(distinct(pdo.Person))
		from dbo.PersonDayOff pdo
		left join #LastKnownBusinessUnit p
			on pdo.Person = p.Person --106469
		where p.person is null
	)
	BEGIN
		PRINT char(13)
		PRINT '----'
		PRINT char(9) + 'Warning - Some PersonDayOff belong to a person without team/site/businessUnit'
		PRINT char(9) + '@PersonsWithoutBu: ' +cast(@PersonsWithoutBu as nvarchar(10))
		PRINT '----'
	END

	;WITH DayOffTemplateSameName AS
	(
	select
		dot.Id,
		dot.CreatedOn,
		dot.Name,
		dot.BusinessUnit,
		ROW_NUMBER() OVER (PARTITION BY dot.Name, dot.BusinessUnit ORDER BY dot.CreatedOn ASC) AS OrderIndex
	from dbo.DayOffTemplate dot
	)
	INSERT INTO #DayOffTemplateSameName
	SELECT
		a.Id,
		convert(datetime,floor(convert(decimal(18,8),a.CreatedOn))),
		a.Name,
		a.BusinessUnit,
		CASE WHEN b.CreatedOn IS NULL THEN '2059-12-31' ELSE convert(datetime,floor(convert(decimal(18,8),b.CreatedOn))) END as 'ValidTo'
	FROM DayOffTemplateSameName a
	LEFT JOIN DayOffTemplateSameName b
		ON a.BusinessUnit = b.BusinessUnit
		AND a.Name = b.Name collate database_default
		AND a.OrderIndex = b.OrderIndex-1
	ORDER BY a.BusinessUnit,a.Name,a.OrderIndex

	--Attach DayOff that belongs to an existing Template "period" by assignment Date, use between for Template vs. Date
	--1) 2657
	update pa SET 
	[Version]		= pa.Version + 1,
	[UpdatedBy]		= @superUser,
	[UpdatedOn]		= getutcdate(),
	[DayOffTemplate]= dot.Id
	from dbo.PersonDayOff pdo
	inner join dbo.PersonAssignment pa
		on pa.Person = pdo.Person
		and pa.Date = convert(datetime,floor(convert(decimal(18,8),pdo.anchor)))
	inner join #DayOffTemplateSameName dot
		on pdo.Name = dot.Name collate database_default
		and dot.BusinessUnit = pdo.Businessunit
		and pa.Date between dot.CreatedOn and dot.ValidToDate
	
		SELECT @ThisStep=@@ROWCOUNT
		PRINT 'Step 1) - Add DayOff where Assignment already existed and we also have valid DayOffTemplate: ' + cast(@ThisStep as nvarchar(10))
		SELECT @Converted = @Converted + @ThisStep

	--Catch up all personAssignment we didn't get a hit by last update
	--If we can't find a matching Template period, just connect them to the "current" DayOffTemplate with correct Name and BU
	--2) 1455
	update pa SET 
	[Version]		= pa.Version + 1,
	[UpdatedBy]		= @superUser,
	[UpdatedOn]		= getutcdate(),
	[DayOffTemplate]= dot.Id
	from dbo.PersonDayOff pdo
	inner join dbo.PersonAssignment pa
		on pa.Person = pdo.Person
		and pa.Date = convert(datetime,floor(convert(decimal(18,8),pdo.anchor)))
	inner join #DayOffTemplateSameName dot
		on pdo.Name = dot.Name collate database_default
		and dot.BusinessUnit = pdo.Businessunit
		and dot.ValidToDate='2059-12-31'
	where pa.DayOffTemplate is null

		SELECT @ThisStep=@@ROWCOUNT
		PRINT 'Step 2) - Add DayOff where Assignment already existed but Date is "before" DayOffTemplate was created: ' + cast(@ThisStep as nvarchar(10))
		SELECT @Converted = @Converted + @ThisStep

	-- Insert new DayOff with no aligned Assignments (by Date)
	-- Assume the Template to use is the last=current.
	-- #24439 - handle duplicate rows
	--3) 129040
		INSERT INTO [dbo].[PersonAssignment]
		SELECT DISTINCT
		newid(),
		[Version]		= 1,
		[CreatedBy]		= cast(max(cast(pdo.CreatedBy AS BINARY(16)))as uniqueidentifier),
		[UpdatedBy]		= cast(max(cast(pdo.UpdatedBy AS BINARY(16)))as uniqueidentifier),
		[CreatedOn]		= max(pdo.CreatedOn),
		[UpdatedOn]		= max(pdo.UpdatedOn),
		[Person]		= pdo.Person,
		[Scenario]		= pdo.scenario,
		[BusinessUnit]	= cast(max(cast(pdo.BusinessUnit AS BINARY(16)))as uniqueidentifier),
		[Date]			= convert(datetime,floor(convert(decimal(18,8),pdo.anchor))),
		[ShiftCategory]	= NULL,
		[DayOffTemplate]= cast(max(cast(dot.Id AS BINARY(16)))as uniqueidentifier)
 
		from dbo.PersonDayOff pdo
		inner join #DayOffTemplateSameName dot
			on pdo.Name = dot.Name collate database_default
			and dot.BusinessUnit = pdo.Businessunit
			and dot.ValidToDate='2059-12-31'
		WHERE NOT EXISTS (
			select 1 from dbo.PersonAssignment pa
			where pa.Person = pdo.Person
			and pa.Date = convert(datetime,floor(convert(decimal(18,8),pdo.anchor)))
		)
		GROUP BY pdo.Person,pdo.scenario,convert(datetime,floor(convert(decimal(18,8),pdo.anchor)))

		SELECT @ThisStep=@@ROWCOUNT
		PRINT 'Step 3) - new Days off, no existing Assignment on this Date: ' + cast(@ThisStep as nvarchar(10))
		SELECT @Converted = @Converted + @ThisStep

	--====================
	--END
	PRINT 'Converted DayOff :' + cast(@Converted as nvarchar(10))
	SELECT @Assert as 'Assert',@Converted as 'Test'
	RETURN 0
	--====================
	
	--4) 
	--recreate "historic" Templates no longer to find
	INSERT INTO [dbo].[DayOffTemplate]
	SELECT
	newid()
	,[Version] = 1
	,[CreatedBy] = @superUser
	,[UpdatedBy] = @superUser
	,[CreatedOn] = getutcdate()
	,[UpdatedOn] = getutcdate()
	,[Name]		 = pdo.Name
	,[ShortName] = pdo.ShortName
	,[Flexibility] = pdo.Flexibility
	,[Anchor] = datediff(SECOND,convert(smalldatetime,floor(convert(decimal(18,8),pdo.anchor))),pdo.anchor)
--	,[Anchor] = pdo.anchor
	,[TargetLength] = pdo.TargetLength
	,[BusinessUnit] = pdo.BusinessUnit
	,[IsDeleted] = 1
	,[DisplayColor] = pdo.DisplayColor
	,[PayrollCode] = pdo.PayrollCode
	FROM dbo.PersonDayOff pdo
	WHERE NOT EXISTS (
		SELECT 1 FROM dbo.DayOffTemplate dot
		WHERE	dot.Name			= pdo.Name
		AND		dot.BusinessUnit	= pdo.BusinessUnit
		)
	GROUP BY
		pdo.Name,
		pdo.ShortName,
		pdo.Flexibility,
		datediff(SECOND,convert(smalldatetime,floor(convert(decimal(18,8),pdo.anchor))),pdo.anchor),
--		pdo.anchor,
		pdo.TargetLength,
		pdo.BusinessUnit,
		pdo.DisplayColor,
		pdo.PayrollCode

	truncate table #DayOffTemplateSameName
	;WITH DayOffTemplateSameName AS
	(
	select
		dot.Id,
		dot.CreatedOn,
		dot.Name,
		dot.BusinessUnit,
		ROW_NUMBER() OVER (PARTITION BY dot.Name, dot.BusinessUnit ORDER BY dot.CreatedOn ASC) AS OrderIndex
	from dbo.DayOffTemplate dot
	)
	INSERT INTO #DayOffTemplateSameName
	SELECT
		a.Id,
		convert(datetime,floor(convert(decimal(18,8),a.CreatedOn))),
		a.Name,
		a.BusinessUnit,
		CASE WHEN b.CreatedOn IS NULL THEN '2059-12-31' ELSE convert(datetime,floor(convert(decimal(18,8),b.CreatedOn))) END as 'ValidTo'
	FROM DayOffTemplateSameName a
	LEFT JOIN DayOffTemplateSameName b
		ON a.BusinessUnit = b.BusinessUnit
		AND a.Name = b.Name
		AND a.OrderIndex = b.OrderIndex-1
	ORDER BY a.BusinessUnit,a.Name,a.OrderIndex

	--init after conversion
	declare @count int
	declare @auditOn int

	select @count = count(*) from [Auditing].[Revision]
	select @auditOn = IsScheduleEnabled from [Auditing].[AuditSetting] where Id=1

	if (@count=0)
	begin
		if (@auditOn=1)
		begin
			exec [Auditing].[InitAuditTables]
		end
	end
END
GO
--EXEC [dbo].[DayOffConverter]