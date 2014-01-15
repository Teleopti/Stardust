declare @SourceId int
declare @DataSourceId int
declare @BusinessUnit uniqueidentifier
declare @teamId uniqueidentifier

--===EDIT===
--1) SQLCMD mode
--Not run this script in SQLCMD-mode. see "Query"-menu

--2) your app db
USE REPLACE_TO_MATCH_APP_DB

--3) your analytics db
--change 
:setvar analytics REPLACE_TO_MATCH_ANALYTICS_DB

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


--4) team.Id
--set this values to match the team_code (aka team.Id) you would like to send RTA events to
set @teamId = '2448F8CA-8082-455B-BF60-A1490101AA29' 
--==========

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
inner join RtaState rs
	on rs.Parent = rg.Id
where t.Id = @teamId
and ex.DataSourceId = @DataSourceId

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