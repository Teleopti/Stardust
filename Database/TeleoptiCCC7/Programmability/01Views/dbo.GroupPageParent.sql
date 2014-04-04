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