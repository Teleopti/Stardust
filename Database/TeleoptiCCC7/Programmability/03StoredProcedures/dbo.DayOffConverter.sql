IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DayOffConverter]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DayOffConverter]
GO
-- =============================================
-- Author:		DJ
-- Create date: 2013-08-26
-- Description:	Called by Security.exe to convert DayOff to new ScheduleDay design
-- ---------------------------------------------
-- When			Who	Change
-- ---------------------------------------------
-- 2013-09-04	DJ	skip periods for templates, just get last
-- 2013-09-05	DJ	remove RAISERROR, added warning instead
-- =============================================
CREATE PROCEDURE [dbo].[DayOffConverter]
AS
SET NOCOUNT ON
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PersonDayOff]') AND type in (N'U'))
BEGIN

--debug
--reset DayOff
/*
DELETE FROM personAssignment WHERE ShiftCategory IS null
UPDATE personAssignment SET DayOffTemplate = null WHERE DayOffTemplate IS not null
DELETE FROM dbo.dayOffTemplate WHERE UpdatedBy='3F0886AB-7B25-4E95-856A-0D726EDC2A67'
DELETE FROM dbo.DatabaseVersion WHERE BuildNumber=-18589

BEGIN TRAN
	EXEC [dbo].[DayOffConverter]
ROLLBACK TRAN
*/

	--temp table used
	CREATE TABLE #personTeam(Person uniqueidentifier,Team uniqueidentifier,StartDate DateTime, OrderIndex int)
	CREATE CLUSTERED INDEX IX_Person ON #personTeam	([Person])

	CREATE TABLE #LastKnownBusinessUnit(Person uniqueidentifier,Businessunit uniqueidentifier)
	CREATE NONCLUSTERED INDEX IX_Person_Businessunit
	ON #LastKnownBusinessUnit ([Person]) INCLUDE ([BusinessUnit])

	CREATE TABLE #DayOffTemplateSameName(Id uniqueidentifier, Name nvarchar(50), BusinessUnit uniqueidentifier)
	--declare
	DECLARE @Assert int
	DECLARE @PersonsMultipleBu int
	DECLARE @PersonsWithoutBu int
	DECLARE @superUser uniqueidentifier
	DECLARE @Converted int
	DECLARE @ThisStep int
	DECLARE @BuildNumber int
	DECLARE @SystemVersion varchar(100)
	DECLARE @ErrorMsg nvarchar(4000)

	--init
	SET @Assert=0
	SET @PersonsMultipleBu=0
	SET @PersonsWithoutBu=0
	SET @superUser='3f0886ab-7b25-4e95-856a-0d726edc2a67'
	SET @Converted=0
	SET @ThisStep=0
	SET @BuildNumber=-18589
	SET @SystemVersion=''
	SET @ErrorMsg = ''

	PRINT char(9)

	--Convert only once
	IF EXISTS (SELECT 1 FROM dbo.DatabaseVersion WHERE BuildNumber=@BuildNumber)
	BEGIN
		SELECT @ErrorMsg='DayOffs already converted, do nothing.'
		PRINT char(9) + char(9) + char(9) + char(9) + @ErrorMsg
		RETURN 0
	END
	
	SELECT top 1 @SystemVersion=SystemVersion+'.1' FROM dbo.DatabaseVersion ORDER BY BuildNumber

	--get the number of dayOff to convert
	select @Assert = count(person) from (
		select distinct Person, Scenario,convert(datetime,floor(convert(decimal(18,8),anchor))) as 'Date'
		from dbo.PersonDayOff
		group by Person, Scenario,convert(datetime,floor(convert(decimal(18,8),anchor)))
	) a
	PRINT char(9) + 'Total number of DayOff to be converted: ' + +cast(@Assert as nvarchar(10))

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
		PRINT char(9) + 'WARNING: Some agents have belonged to more than one business unit'

	--DayOff connected to a person without Person Period = No Business Unit
	select @PersonsWithoutBu=
	(
		select count(distinct(pdo.Person))
		from dbo.PersonDayOff pdo
		left join #LastKnownBusinessUnit p
			on pdo.Person = p.Person --106469
		where p.person is null
	)
	IF @PersonsWithoutBu>0
		PRINT char(9) + 'WARNING: Some DayOffs belong to persons without valid person period'

	--Recreate DayOffTemplate that is no longer to find by Name
	INSERT INTO [dbo].[DayOffTemplate]
	SELECT
	newid()
	,[Version] = 1
	,[UpdatedBy] = @superUser
	,[UpdatedOn] = getutcdate()
	,[Name]		 = pdo.Name
	,[ShortName] = pdo.ShortName
	,[Flexibility] = pdo.Flexibility
	,[Anchor] = datediff(SECOND,convert(smalldatetime,floor(convert(decimal(18,8),pdo.anchor))),pdo.anchor)
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
		pdo.TargetLength,
		pdo.BusinessUnit,
		pdo.DisplayColor,
		pdo.PayrollCode

	--Create a temporary table that holds the last DayOffTemplate
	;WITH DayOffTemplateSameName AS
	(
	select
		dot.Id,
		dot.UpdatedOn,
		dot.Name,
		dot.BusinessUnit,
		ROW_NUMBER() OVER
			(
			PARTITION BY
				dot.Name,
				dot.BusinessUnit
			ORDER BY dot.UpdatedOn DESC
			) AS OrderIndex
	from dbo.DayOffTemplate dot
	)
	INSERT INTO #DayOffTemplateSameName
	SELECT
		a.Id,
		a.Name,
		a.BusinessUnit
	FROM DayOffTemplateSameName a
	WHERE OrderIndex=1

	--Step 1) - Add DayOff where Assignment already existed with perfecty match on person, date and scenario
	update pa SET 
	[Version]		= pa.Version + 1,
	[UpdatedBy]		= @superUser,
	[UpdatedOn]		= getutcdate(),
	[DayOffTemplate]= dot.Id
	from dbo.PersonDayOff pdo
	inner join dbo.PersonAssignment pa
		on pa.Person = pdo.Person
		and pa.Date = convert(datetime,floor(convert(decimal(18,8),pdo.anchor)))
		and pa.Scenario = pdo.Scenario
	inner join #DayOffTemplateSameName dot
		on pdo.Name = dot.Name collate database_default
		and dot.BusinessUnit = pdo.Businessunit

	SELECT @ThisStep=@@ROWCOUNT
	PRINT char(9) + 'DayOff on days with existing shift: ' + cast(@ThisStep as nvarchar(10))
	SELECT @Converted = @Converted + @ThisStep

	--Step 2) - Existing Days off on new Assignment dates
		INSERT INTO [dbo].[PersonAssignment]
		SELECT DISTINCT
		newid(),
		[Version]		= 1,
		[UpdatedBy]		= cast(max(cast(pdo.UpdatedBy AS BINARY(16)))as uniqueidentifier),
		[UpdatedOn]		= max(pdo.UpdatedOn),
		[Person]		= pdo.Person,
		[Scenario]		= pdo.scenario,
		[Date]			= convert(datetime,floor(convert(decimal(18,8),pdo.anchor))),
		[ShiftCategory]	= NULL,
		[DayOffTemplate]= cast(max(cast(dot.Id AS BINARY(16)))as uniqueidentifier)
		from dbo.PersonDayOff pdo
		inner join #DayOffTemplateSameName dot
			on pdo.Name = dot.Name collate database_default
			and dot.BusinessUnit = pdo.Businessunit
		WHERE NOT EXISTS (
			select 1 from dbo.PersonAssignment pa
			where pa.Person = pdo.Person
			and pa.Date = convert(datetime,floor(convert(decimal(18,8),pdo.anchor)))
			and pa.Scenario = pdo.Scenario
		)
		GROUP BY pdo.Person,pdo.scenario,convert(datetime,floor(convert(decimal(18,8),pdo.anchor)))

		SELECT @ThisStep=@@ROWCOUNT
		PRINT char(9) + 'DayOff on days without shift: ' + cast(@ThisStep as nvarchar(10))
		SELECT @Converted = @Converted + @ThisStep

	IF @Assert<>@Converted
	BEGIN
		SELECT @ErrorMsg= char(9)+ 'WARNING: The number of DayOff converted did not match' + char(13)
		+ char(9) + 'Expected: ' + cast(@Assert as nvarchar(10)) +' but was: ' + cast(@Converted as nvarchar(10))
		PRINT @ErrorMsg
		WAITFOR DELAY '00:00:05'
		--RAISERROR (@ErrorMsg,16,1)
	END

	--To be dropped when customer goes from CCC 7 to CCC 8
	EXEC dbo.sp_rename @objname = N'[dbo].[PersonDayOff]', @newname = N'PersonDayOff_old', @objtype = N'OBJECT'
	EXEC dbo.sp_rename @objname = N'[Auditing].[PersonDayOff_AUD]', @newname = N'PersonDayOff_AUD_old', @objtype = N'OBJECT'

	PRINT char(9) + 'Total number of DayOff converted:' + cast(@Converted as nvarchar(10))

	INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (@BuildNumber,@SystemVersion) 

	RETURN (@Converted-@Assert)

END
GO

