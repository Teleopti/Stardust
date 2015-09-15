IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Purge]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Purge]
GO

-- =============================================
-- Author:		<AF>
-- Create date: <2012-02-02>
-- Description:	<Purge old data from app db>
-- =============================================
-- Changed		Who			Why
-- =============================================
-- 2013-04-29	David J		Re-factor table into key/value, added Security Audit
-- 2014-04-16	Johan R		Removing purge of PayrollExport, is needed for automatic Payrolls
-- =============================================
CREATE PROCEDURE [dbo].[Purge]
AS
BEGIN
declare @KeepUntil datetime
declare @BatchSize int
declare @MaxDate datetime
declare @SuperRole uniqueidentifier
declare @Start smalldatetime

set @start = getdate()
set @SuperRole='193AD35C-7735-44D7-AC0C-B8EDA0011E5F'
set @BatchSize = 14 --Used to control number of days to delete in one go.

create table #Deleted (Id uniqueidentifier)

/*
exec Purge
*/

--Set up (i.e. skip migration scripts just for the purpose of populating the config table)
if not exists (select 1 from PurgeSetting where [key] = 'YearsToKeepForecast')
	insert into PurgeSetting ([Key], [Value]) values ('YearsToKeepForecast', 10)
if not exists (select 1 from PurgeSetting where [key] = 'YearsToKeepMessage')
	insert into PurgeSetting ([Key], [Value]) values ('YearsToKeepMessage', 10)
if not exists (select 1 from PurgeSetting where [key] = 'YearsToKeepPayroll')
	insert into PurgeSetting ([Key], [Value]) values ('YearsToKeepPayroll', 20)
if not exists (select 1 from PurgeSetting where [key] = 'YearsToKeepSchedule')
	insert into PurgeSetting ([Key], [Value]) values ('YearsToKeepSchedule', 10)
if not exists (select 1 from PurgeSetting where [key] = 'DaysToKeepSecurityAudit')
	insert into PurgeSetting ([Key], [Value]) values ('DaysToKeepSecurityAudit', 30)
if not exists (select 1 from PurgeSetting where [key] = 'MonthsToKeepRequests')
	insert into PurgeSetting ([Key], [Value]) values ('MonthsToKeepRequests', 120)
if not exists (select 1 from PurgeSetting where [key] = 'DenyPendingRequestsAfterNDays')
	insert into PurgeSetting ([Key], [Value]) values ('DenyPendingRequestsAfterNDays', 14)
if not exists (select 1 from PurgeSetting where [key] = 'YearsToKeepPersons')
	insert into PurgeSetting ([Key], [Value]) values ('YearsToKeepPersons', 10)

--Persons who has left, i.e. with a since long past leaving date
select @KeepUntil = dateadd(year,-1*(select isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepPersons'),getdate())

update person set IsDeleted = 1
where isnull(TerminalDate,'20591231') < @KeepUntil

insert into #Deleted
select top(20) p.Id
from Person p
where p.IsDeleted = 1
and exists (select 1 from PersonPeriod pp where pp.Parent = p.Id)

delete SchedulePeriodShiftCategoryLimitation
from SchedulePeriodShiftCategoryLimitation scl
inner join SchedulePeriod sp on scl.SchedulePeriod = sp.Id
inner join #Deleted p on sp.Parent = p.Id

delete SchedulePeriod
from SchedulePeriod sp
inner join #Deleted p on sp.Parent = p.Id

delete ExternalLogOnCollection
from ExternalLogOnCollection ex
inner join PersonPeriod pp on ex.PersonPeriod = pp.Id
inner join #Deleted p on pp.Parent = p.Id

delete PersonSkill
from PersonSkill ps
inner join PersonPeriod pp on ps.Parent = pp.Id
inner join #Deleted p on pp.Parent = p.Id

delete PersonPeriod
from PersonPeriod pp
inner join #Deleted p on pp.Parent = p.Id

delete top (100) Auditing.Revision
from Auditing.Revision r
inner join Auditing.PersonAssignment_AUD pa on pa.REV = r.Id
inner join person p on pa.Person = p.Id
where p.IsDeleted = 1

delete top (100) Auditing.Revision
from Auditing.Revision r
inner join Auditing.PersonAbsence_AUD pa on pa.REV = r.Id
inner join person p on pa.Person = p.Id
where p.IsDeleted = 1

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

/*
exec Purge
*/

--Forecast
select @KeepUntil = dateadd(year,-1*(select isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepForecast'),getdate())

select @MaxDate = dateadd(day,@BatchSize,isnull(min(WorkloadDate),'19900101'))
from WorkloadDayBase wdb
inner join WorkloadDay wd on wdb.id = wd.WorkloadDayBase
where wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete TemplateTaskPeriod
from TemplateTaskPeriod ttp
inner join WorkloadDayBase wdb on ttp.Parent = wdb.Id
inner join WorkloadDay wd on wdb.id = wd.WorkloadDayBase
where 1=1
and wdb.WorkloadDate < @KeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete WorkloadDay 
from WorkloadDay wd
inner join WorkloadDayBase wdb on wdb.id = wd.WorkloadDayBase
where 1=1
and wdb.WorkloadDate < @KeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete OpenhourList
from OpenhourList ol
inner join WorkloadDayBase wdb on wdb.id = ol.Parent
where 1=1
and wdb.WorkloadDate < @KeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates


delete WorkloadDayBase
from WorkloadDayBase wdb
where 1=1
and Not exists (select 1 from WorkloadDayTemplate wdt
				where wdt.WorkloadDayBase = wdb.Id) --At one customer I found templates on incorrect dates, avoid those and do not try to fix them here in the purge.
and wdb.WorkloadDate < @KeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete SkillDataPeriod
from SkillDataPeriod sdp
inner join SkillDay sd on sdp.Parent = sd.Id
where 1=1
and sd.SkillDayDate < @KeepUntil
and sd.SkillDayDate < @MaxDate

delete SkillDay
from SkillDay sd
where 1=1
and sd.SkillDayDate < @KeepUntil
and sd.SkillDayDate < @MaxDate

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

--Schedule
select @KeepUntil = dateadd(year,-1*(select isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepSchedule'),getdate())
select @MaxDate = dateadd(day,@BatchSize,isnull(min(Date),'19900101')) from PersonAssignment

delete PersonAssignment --Lovely, cascade delete is on...
from PersonAssignment pa
where 1 = 1
and pa.Date < @KeepUntil
and pa.Date < @MaxDate

delete PersonAbsence --Lovely, cascade delete is on...
from PersonAbsence pa
where 1 = 1
and pa.Maximum < @KeepUntil
and pa.Maximum < @MaxDate

delete Note
from Note n
where 1 = 1
and n.NoteDate < @KeepUntil
and n.NoteDate < @MaxDate

delete AgentDayScheduleTag
from AgentDayScheduleTag ad
where 1 = 1
and ad.TagDate < @KeepUntil
and ad.TagDate < @MaxDate

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

--Remove deleted skills from persons
delete PersonSkill
from PersonSkill ps
inner join Skill s on ps.Skill = s.Id
where s.IsDeleted = 1

--Remove roles on deleted persons to prevent them from showing up in Permissions
--Don't remove permissions for super user because they are deleted
delete PersonInApplicationRole
from PersonInApplicationRole piar
inner join Person p on p.Id = piar.Person
where p.IsDeleted = 1 and piar.ApplicationRole <> @SuperRole

--Remove deleted budget groups from persons
update PersonPeriod set BudgetGroup = NULL
from PersonPeriod pp
inner join BudgetGroup bg on pp.BudgetGroup = bg.Id
where bg.IsDeleted = 1

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

--Messages
select @KeepUntil = dateadd(year,-1*(select isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepMessage'),getdate())
select @MaxDate = dateadd(day,@BatchSize,isnull(min(UpdatedOn),'19900101')) from PushMessageDialogue

delete DialogueMessage
from DialogueMessage dm
inner join PushMessageDialogue pmd on pmd.Id = dm.Parent
where pmd.UpdatedOn < @KeepUntil
and pmd.UpdatedOn < @MaxDate

delete PushMessageDialogue
from PushMessageDialogue pmd
where pmd.UpdatedOn < @KeepUntil
and pmd.UpdatedOn < @MaxDate

delete ReplyOptions
from ReplyOptions ro
inner join PushMessage pm on ro.id = pm.Id
where not exists (select 1 from PushMessageDialogue pmd where pmd.PushMessage = pm.Id)

delete PushMessage
from PushMessage pm
where not exists (select 1 from PushMessageDialogue pmd where pmd.PushMessage = pm.Id)

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

--Payroll
select @KeepUntil = dateadd(year,-1*(select isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepPayroll'),getdate())
select @MaxDate = dateadd(day,@BatchSize,isnull(min(UpdatedOn),'19900101')) from PayrollResult

delete PayrollResultDetail
from PayrollResultDetail prd
inner join PayrollResult pr on prd.Parent = pr.Id
where pr.UpdatedOn < @KeepUntil
and pr.UpdatedOn < @Maxdate

delete PayrollResult
from PayrollResult pr
where pr.UpdatedOn < @KeepUntil
and pr.UpdatedOn < @Maxdate

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

--Requests
--AF: Need to remove top (50000) as this caused corrupt data...
select @KeepUntil = dateadd(month,-1*(select isnull(Value,100) from PurgeSetting where [Key] = 'MonthsToKeepRequests'),getdate())

update ShiftTradeRequest
set Offer = NULL
from ShiftTradeRequest a 
inner join ShiftExchangeOffer b on a.Offer=b.Request
inner join Request r on r.id = b.Request
where r.EndDateTime < @KeepUntil
and a.Offer is not null

delete ShiftExchangeOffer 
from ShiftExchangeOffer a
inner join Request r on r.id = a.Request
where r.EndDateTime < @KeepUntil

delete ShiftTradeSwapDetail
from ShiftTradeSwapDetail a
inner join ShiftTradeRequest st on st.Request = a.Parent
inner join Request r on r.id = st.Request
where r.EndDateTime < @KeepUntil

delete ShiftTradeRequest
from ShiftTradeRequest a
inner join Request r on r.id = a.Request
where r.EndDateTime < @KeepUntil
and not exists (
	select 1 from ShiftTradeSwapDetail stsd
	where stsd.Parent = a.Request)

delete AbsenceRequest
from AbsenceRequest a
inner join Request r on r.id = a.Request
where r.EndDateTime < @KeepUntil

delete TextRequest
from TextRequest a
inner join Request r on r.id = a.Request
where r.EndDateTime < @KeepUntil

delete Request
from Request r
where r.EndDateTime < @KeepUntil

delete PersonRequest
from PersonRequest pr
where not exists (
select 1 from Request r
where r.Parent = pr.Id)

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

--Autodeny requests if not handled in time.
select @KeepUntil = dateadd(day,-1*(select isnull(Value,120) from PurgeSetting where [Key] = 'DenyPendingRequestsAfterNDays'),getdate())

update PersonRequest set RequestStatus = 1
from PersonRequest pr inner join Request r on r.Parent = pr.Id
where r.EndDateTime < @KeepUntil
and pr.RequestStatus = 0 --Pending
and pr.IsDeleted = 0

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

--New Adherence read models. Purge for now since we have not yet built or tested with lots of historical data.
delete ReadModel.AdherencePercentage
where BelongsToDate < dateadd(day,-1,getdate())

if datediff(second,@start,getdate()) > 240 --Because timeout from ETL is 5 mins
	return

delete ReadModel.AdherenceDetails
where BelongsToDate < dateadd(day,-1,getdate())

END

GO


