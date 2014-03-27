declare @SourceId int
declare @DataSourceId int
declare @BusinessUnit uniqueidentifier
declare @teamIdSet nvarchar(max)
declare @siteIdSet nvarchar(max)


--=============EDIT=============
--Note: you need to run this script in "SQLCMD"-mode. see "Query"-menu

-- your app db
USE EDIT_YOUR_CCC7_DB

--your analytics db
:setvar analytics EDIT_YOUR_ANALYTICS_DB

--site and team ids as comma seperated strings
set @teamIdSet = '0A1CDB27-BC01-4BB9-B0B3-9B5E015AB495,143E088C-B6F4-453C-9656-A0A200DA099A'
set @siteIdSet = 'D970A45A-90FF-4111-BFE1-9B5E015AB45C,E8428339-BBF1-456B-8DE7-BB2208D93073,7BB69C40-AAE8-4E82-BA3D-449284F56ED6'
--==============================

IF OBJECT_ID('tempdb.dbo.#teams') IS NOT NULL DROP TABLE #teams
CREATE TABLE #teams (id uniqueidentifier)

IF OBJECT_ID('tempdb.dbo.#sites') IS NOT NULL DROP TABLE #sites
CREATE TABLE #sites (id uniqueidentifier)

INSERT #teams
	SELECT * FROM mart.SplitStringString(@teamIdSet)

INSERT #sites
	SELECT * FROM mart.SplitStringString(@siteIdSet)
--show all available teams
select bu.Name 'BusinessUnit',s.Name 'Site', s.Id 'Site Id', t.Name 'Team',t.Id as 'Team Id',count(p.Id) 'number of agents'
from BusinessUnit bu
inner join site s
	on s.BusinessUnit = bu.Id
inner join Team t
	on t.Site	=s.Id
inner join PersonPeriodWithEndDate pp
	on pp.Team = t.Id
	and getdate() between pp.StartDate and pp.EndDate
inner join Person p
	on p.Id = pp.Parent
group by bu.Name,s.Name,s.Id,t.Name,t.Id
order by bu.Name,s.Name,T.Name

IF OBJECT_ID('tempdb.dbo.#t') IS NOT NULL DROP TABLE #t
create table #t (BusinessUnit uniqueidentifier, SourceId int, PlatformTypeId char(36), LogOn nvarchar(50), Person char(36), StateCode nvarchar(25))

--get the business unit id
select @BusinessUnit=bu.Id
from BusinessUnit bu
inner join site s
	on s.BusinessUnit = bu.Id
inner join Team t
	on t.Site	=s.Id
inner join PersonPeriodWithEndDate pp
	on pp.Team = t.Id
	and getdate() between pp.StartDate and pp.EndDate
inner join Person p
	on p.Id = pp.Parent
where 	(t.Id in (select Id from #teams) or s.id in (select Id from #sites))

--get the most frequent datasource connected to this team
select top 1 @DataSourceId = ex.DataSourceId
from BusinessUnit bu
inner join site s
	on s.BusinessUnit = bu.Id
inner join Team t
	on t.Site	=s.Id
inner join PersonPeriodWithEndDate pp
	on pp.Team = t.Id
	and getdate() between pp.StartDate and pp.EndDate
inner join Person p
	on p.Id = pp.Parent
inner join ExternalLogOnCollection exc
	on exc.PersonPeriod = pp.Id
inner join ExternalLogOn ex
	on ex.Id = exc.ExternalLogOn
inner join RtaStateGroup rg
	on rg.BusinessUnit = bu.Id
inner join RtaState rs
	on rs.Parent = rg.Id
where 	(t.Id in (select Id from #teams) or s.id in (select Id from #sites))
GROUP BY ex.DataSourceId
ORDER BY count(DataSourceId) desc

insert into #t 
select
	BusinessUnit	= bu.Id,
	SourceId		= ex.DataSourceId,
	PlatformTypeId	= cast(rs.PlatformTypeId as char(36)),
	LogOn			= ex.AcdLogOnOriginalId,
	Person			 = cast(p.Id as char(36)),
	StateCode		= rs.StateCode
from BusinessUnit bu
inner join site s
	on s.BusinessUnit = bu.Id
inner join Team t
	on t.Site	=s.Id
inner join PersonPeriodWithEndDate pp
	on pp.Team = t.Id
	and getdate() between pp.StartDate and pp.EndDate
inner join Person p
	on p.Id = pp.Parent
inner join ExternalLogOnCollection exc
	on exc.PersonPeriod = pp.Id
inner join ExternalLogOn ex
	on ex.Id = exc.ExternalLogOn
inner join RtaStateGroup rg
	on rg.BusinessUnit = bu.Id
	and rg.IsDeleted=0
inner join RtaState rs
	on rs.Parent = rg.Id
where
	(t.Id in (select Id from #teams) or s.id in (select Id from #sites))
and ex.DataSourceId = @DataSourceId
and rs.Id in (
	select a.Id from (
		SELECT
			a.Id,
			ROW_NUMBER() over(PARTITION BY b.Id ORDER BY b.Name ASC) rowNumber
		from RtaState a
		inner join RtaStateGroup b
			on a.parent = b.Id
		where b.IsDeleted=0
			) a
	where a.rowNumber=1
	)

--fixed values
select @BusinessUnit as BusinessUnit
select source_id FROM [$(analytics)].mart.sys_datasource where datasource_id=@DataSourceId

--PersonIdsForScheduleUpdate
declare @text xml

select (
	SELECT distinct t1.person + ','
	FROM #t t1
	FOR XML PATH('')
	) as [Persons]

--LogOn
select (
SELECT distinct t1.LogOn + ',' 
FROM #t t1
FOR XML PATH('')
	) as [Logon]

--StateCode
select (
	SELECT distinct t1.StateCode + ',' 
	FROM #t t1
	FOR XML PATH('')
	) as [StateCode]