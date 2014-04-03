
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

----------------  
--Name: Anders Forsberg
--Date: 2013-12-05
--Desc: Feature #26164 - Add purge of old requests to purge routine
---------------- 
if not exists (select 1 from PurgeSetting where [Key] = 'MonthsToKeepRequests')
	insert into PurgeSetting ([Key], [Value]) values('MonthsToKeepRequests', 120)
GO

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