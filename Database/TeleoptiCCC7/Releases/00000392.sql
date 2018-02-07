----------------  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Site]') AND name = N'IX_Site_BusinesUnit')
CREATE NONCLUSTERED INDEX [IX_Site_BusinesUnit] ON [dbo].[Site]
(
	[BusinessUnit] ASC
)
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Team]') AND name = N'IX_Team_Site')
CREATE NONCLUSTERED INDEX [IX_Team_Site] ON [dbo].[Team]
(
	[Site] ASC
)
GO

--Name: Henrik Andersson
--Date: 2013-12-16  
--Desc: Add the following new application function> MyReportWeb
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0090' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'Raptor/MyTimeWeb/MyReportWeb' --Name of the function > hardcoded
SELECT @FunctionDescription = 'MyReportWeb' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Anders Forsberg
--Date: 2013-12-05
--Desc: Feature #26164 - Add purge of old requests to purge routine
---------------- 

----------------  
--Name: David Jonsson
--Date: 2014-02-10
--Desc: bug #26903 - make a more simple version of PersenPeriodWithEndDate for RTA
---------------- 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonPeriod]') AND name = N'IX_PersonPeriod_Parent_StartDate_Id')
CREATE NONCLUSTERED INDEX [IX_PersonPeriod_Parent_StartDate_Id] ON [dbo].[PersonPeriod]
(
	[Parent] ASC,
	[StartDate] ASC
)
INCLUDE ([Id])
GO

----------------  
--Name: Erik Sundberg
--Date: 2014-01-25
--Desc: Add the following new application function> RealTimeAdherenceOverview
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0080'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0092' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'Raptor/Anywhere/RealTimeAdherenceOverview' --Name of the function > hardcoded
SELECT @FunctionDescription = 'RealTimeAdherenceOverview' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Chundan Xu
--Date: 2014-03-11  
--Desc: Add the following new application function> view personal account in MyTimeWeb
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0065'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0093' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'ViewPersonalAccount' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxViewPersonalAccount' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: Jiajun Qiu
--Date: 2014-03-25
--Desc: bug #27321 - Remove Grouping Activity
---------------- 
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Activity_GroupingActivity]') AND parent_object_id = OBJECT_ID(N'[dbo].[Activity]'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT [FK_Activity_GroupingActivity]
GO

IF EXISTS ( --column exist
SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Activity') AND name = N'GroupingActivity')
BEGIN
	IF NOT EXISTS ( --but is not part of any index
	SELECT * FROM sys.index_columns ic
	INNER JOIN sys.columns c
	on c.object_id = ic.object_id
	WHERE c.object_id = ic.object_id
	AND c.object_id = OBJECT_ID(N'dbo.Activity')
	AND c.name = N'GroupingActivity'
	AND c.column_id = ic.column_id
	)
	BEGIN
		ALTER TABLE dbo.Activity DROP COLUMN GroupingActivity
	END
END


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GroupingActivity]') AND type in (N'U'))
DROP TABLE [dbo].[GroupingActivity]
GO


----------------  
--Name: Jonas And Kunning
--Date: 2014-04-03
--Desc: Add the following new application function> SignInAsAnotherUser
----------------  
SET NOCOUNT ON
	
--declarations
DECLARE @SuperUserId as uniqueidentifier
DECLARE @FunctionId as uniqueidentifier
DECLARE @ParentFunctionId as uniqueidentifier
DECLARE @ForeignId as varchar(255)
DECLARE @ParentForeignId as varchar(255)
DECLARE @FunctionCode as varchar(255)
DECLARE @FunctionDescription as varchar(255)
DECLARE @ParentId as uniqueidentifier

--insert to super user if not exist
SELECT	@SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

-- check for the existence of super user role
IF  (NOT EXISTS (SELECT id FROM [dbo].[Person] WHERE Id = @SuperUserId)) 
INSERT [dbo].[Person]([Id], [Version], [UpdatedBy],[UpdatedOn], [Email], [Note], [EmploymentNumber], [TerminalDate], [FirstName], [LastName], [DefaultTimeZone], [IsDeleted], [BuiltIn], [FirstDayOfWeek])
VALUES (@SuperUserId,1, @SuperUserId, getdate(), '', '', '', NULL, '_Super User', '_Super User', 'UTC', 0, 1, 1)

--get parent level
SELECT @ParentForeignId = '0023'	--Parent Foreign id that is hardcoded
SELECT @ParentId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ParentForeignId + '%')
	
--insert/modify application function
SELECT @ForeignId = '0094' --Foreign id of the function > hardcoded	
SELECT @FunctionCode = 'SignInAsAnotherUser' --Name of the function > hardcoded
SELECT @FunctionDescription = 'xxSignInAsAnotherUser' --Description of the function > hardcoded
SELECT @ParentId = @ParentId

IF  (NOT EXISTS (SELECT Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')))
INSERT [dbo].[ApplicationFunction]([Id], [Version], [UpdatedBy], [UpdatedOn], [Parent], [FunctionCode], [FunctionDescription], [ForeignId], [ForeignSource], [IsDeleted])
VALUES (newid(),1, @SuperUserId, getdate(), @ParentId, @FunctionCode, @FunctionDescription, @ForeignId, 'Raptor', 0) 
SELECT @FunctionId = Id FROM ApplicationFunction WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')
UPDATE [dbo].[ApplicationFunction] SET [ForeignId]=@ForeignId, [Parent]=@ParentId WHERE ForeignSource='Raptor' AND IsDeleted='False' AND ForeignId Like(@ForeignId + '%')

SET NOCOUNT OFF
GO

----------------  
--Name: David Jonsson
--Date: 2013-12-12
--Desc: Bug #26054 - Remove unique constraint as the read model is update much later by Service bus.
--					 We don't catch the error and ReadModel never updates again
--					 Clean up dupliates in PersonGroup when belong to same "Tab"
---------------- 
IF EXISTS (
	SELECT * FROM sys.indexes
	WHERE object_id = OBJECT_ID(N'[ReadModel].[GroupingReadOnly]')
	AND name = N'UC_GroupingReadOnly'
	)
ALTER TABLE [ReadModel].[GroupingReadOnly] DROP CONSTRAINT UC_GroupingReadOnly
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'dbo.GroupPageParent'))
DROP VIEW dbo.GroupPageParent
GO

create view dbo.GroupPageParent
as
--a) all 1st level e.g. the ones directly under each GroupPage Tab
select
	pgb.Id as 'Id',
	cast(null as uniqueidentifier) as 'Parent',
	rpg.Parent as 'TabId'
from GroupPage gp
inner join RootPersonGroup rpg
	on rpg.Parent = gp.Id
inner join PersonGroupBase pgb
	on pgb.Id = rpg.PersonGroupBase

union all

--b) and childs futher down
select
	pgb.Id as 'Id',
	c.Parent as 'Parent',
	cast(null as uniqueidentifier) as 'TabId'
from PersonGroupBase pgb
left outer join ChildPersonGroup c
	on c.PersonGroupBase=pgb.Id
where not exists (select 1 from RootPersonGroup r where r.PersonGroupBase = pgb.Id)
go

--views added later, needed now
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'dbo.GroupPageHierarchyCTE'))
DROP VIEW dbo.GroupPageHierarchyCTE
GO

-- recursive cte to draw GroupPage hierarchy
create view dbo.GroupPageHierarchyCTE
as

WITH GroupPageHierarchyCTE(level,PersonGroup,Parent,TabId)
AS (
	SELECT
		1,
		e.Id,
		e.Parent,
		e.TabId
	FROM GroupPageParent e
	WHERE Parent IS NULL

	UNION ALL

	SELECT
		cte.level+1,
		e.Id,
		e.Parent,
		cte.TabId
	FROM GroupPageParent e
	INNER JOIN GroupPageHierarchyCTE cte
		ON e.Parent = cte.PersonGroup
	WHERE e.Parent IS NOT NULL
)
select * from GroupPageHierarchyCTE
GO

--temp table to hold OK and not OK personGroup
declare @personGroup table (person uniqueidentifier, PersonGroup uniqueidentifier, IsOk bit);
insert into @personGroup
select
	a.Person,
	a.PersonGroup,
	case a.RowNumer
		when 1 then 1
		else 0
	end as 'IsOk'
from (
	SELECT
		pg.Person,
		cte.PersonGroup,
		ROW_NUMBER() OVER(PARTITION BY pg.Person,cte.TabId ORDER BY cte.level ASC) AS RowNumer
	FROM GroupPageHierarchyCTE cte
	INNER JOIN PersonGroup pg
		on pg.PersonGroup = cte.PersonGroup
) as a
inner join dbo.Person p
	on a.Person=p.Id

if exists (select 1 from @personGroup where IsOk=0)
begin
	print 'found duplicates in custom Group Page'
	delete pg
	from PersonGroup pg
	inner join @personGroup tmp
		on tmp.person = pg.Person
		and tmp.PersonGroup = pg.PersonGroup
	where tmp.IsOk=0
end
GO

----------------  
--Name: Asad Mirza + David Jonsson
--Date: 2014-04-11
--Desc: Bug #27441 + Bug #27534- Removed duplicated data. Keep latests records
---------------- 
--supporting index for the select and delete
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[StudentAvailabilityDay]') AND name = N'IX_StudentAvailabilityDay_Date_Person')
CREATE NONCLUSTERED INDEX [IX_StudentAvailabilityDay_Date_Person]  ON [dbo].[StudentAvailabilityDay]   
(
	[RestrictionDate] ASC,
	[Person] ASC
)
GO

--Note: This SP will be executed by DBManager right after this Trunk.sql is finished
--  => EXEC dbo.WorkAroundFor27636

----------------  
--Name: MickeD + DavidJ
--Date: 2014-04-23
--Desc: Bug #27661 - missing ShiftCategory values
---------------- 
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PersonAssignment]') AND name = N'UQ_PersonAssignment_Date_Scenario_Person')
BEGIN
create table #tmp_missing_shift_category(paID uniqueidentifier, buID uniqueidentifier)
create table #tmp_bu(buID uniqueidentifier)

declare @SuperUserId uniqueidentifier
declare @ShortName nvarchar(25)
declare @Name nvarchar(50)
set @ShortName='?!'
set @Name='missing category'
set @SuperUserId = '3f0886ab-7b25-4e95-856a-0d726edc2a67'

insert #tmp_missing_shift_category
select distinct
	pa.Id,
	sc.BusinessUnit 
from ShiftLayer sl
inner join PersonAssignment pa
	on pa.Id = sl.Parent
inner join Scenario sc
	on pa.Scenario = sc.Id
where LayerType = 1
and pa.ShiftCategory is null

insert #tmp_bu
select cast(max(cast(buId AS binary(16))) as uniqueidentifier)
from #tmp_missing_shift_category
group by buID

if exists (select * from #tmp_missing_shift_category)
begin
	--insert a new ShiftCategory for each BU if missing
	insert into ShiftCategory(id, Version, UpdatedBy, UpdatedOn, Name, ShortName, DisplayColor, BusinessUnit, IsDeleted)
	select 
	newid(), 
	1, 
	@SuperUserId, 
	getutcdate(),
	@Name,
	@ShortName,
	0,
	buID, 
	1
	from #tmp_bu
 
	--update all PersonAssignment with missing ShiftCategory
	update pa
	set ShiftCategory = sc.Id
	from shiftCategory sc
	inner join #tmp_missing_shift_category tmp 
		on sc.BusinessUnit = tmp.buID
	inner join PersonAssignment pa
		on pa.Id = tmp.paID
	where sc.Name = @Name
	and sc.ShortName = @ShortName
end
END
GO

GO

PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (392,'7.5.392') 
