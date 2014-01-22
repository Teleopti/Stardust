declare @SourceId int
declare @DataSourceId int
declare @BusinessUnit uniqueidentifier
declare @teamId uniqueidentifier

--===EDIT===
-- The output is limited to one StateCode per active StateGroup
--1) SQLCMD mode
--Not run this script in SQLCMD-mode. see "Query"-menu

--2) your app db
USE REPLACE_TO_MATCH_APP_DB

--3) your analytics db
--change 
:setvar analytics REPLACE_TO_MATCH_ANALYTICS_DB

--4) team.Id
--set this values to match the team_code (aka team.Id) you would like to send RTA events to
set @teamId = '34590A63-6331-4921-BC9F-9B5E015AB495' 

--==========
--show all available teams
select bu.Name 'BusinessUnit',s.Name 'Site',t.Name 'Team',t.Id as 'Team Id',count(p.Id) 'number of agents'
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
group by bu.Name,s.Name,t.Name,t.Id
order by 1,2,3

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
where t.Id=@teamId

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
where t.Id = @teamId
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
where t.Id = @teamId
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