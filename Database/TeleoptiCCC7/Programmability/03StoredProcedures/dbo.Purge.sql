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
declare @TheDate smalldatetime
declare @RowCount int
declare @timeout int

set @timeout = 240 --Why not pass from code? Current timeout from RTL is 5 mins I think.
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
if not exists (select 1 from PurgeSetting where [Key] = 'DaysToKeepReadmodels')
	insert into PurgeSetting ([Key], [Value]) values('DaysToKeepReadmodels', 30)
if not exists (select 1 from PurgeSetting where [Key] = 'DaysToKeepJobResultArtifacts')
	insert into PurgeSetting ([Key], [Value]) values('DaysToKeepJobResultArtifacts', 30)

--Persons who has left, i.e. with a since long past leaving date
select @KeepUntil = dateadd(year,-1*(select isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepPersons'),getdate())

update person set IsDeleted = 1
where isnull(TerminalDate,'20591231') < @KeepUntil

while 20 < (select count(1) from Person p inner join personperiod pp on pp.parent = p.id where IsDeleted = 1) --Large customers may have way more than 20 leavers per day
begin
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

	delete ReadModel.PersonScheduleDay
	from ReadModel.PersonScheduleDay ps
	inner join #Deleted d on d.Id = ps.PersonId

	if datediff(second,@start,getdate()) > @timeout 
		return
end

set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) Auditing.PersonAssignment_AUD
	from Auditing.PersonAssignment_AUD pa
	inner join person p on pa.Person = p.Id
	where p.IsDeleted = 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) Auditing.PersonAbsence_AUD
	from Auditing.PersonAbsence_AUD pa
	inner join person p on pa.Person = p.Id
	where p.IsDeleted = 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

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

if datediff(second,@start,getdate()) > @timeout 
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

if datediff(second,@start,getdate()) > @timeout 
	return

--Audit trails must be purged as part of schedules, at least until they get their own setting. Purge based on schedule date, not change date.
set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) Auditing.PersonAssignment_AUD
	from Auditing.PersonAssignment_AUD pa
	where pa.date < @KeepUntil

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end


set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) Auditing.PersonAbsence_AUD
	from Auditing.PersonAbsence_AUD pa
	where pa.maximum < @KeepUntil

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

--Empty revisions?
set @RowCount = 1
while @RowCount > 0
begin
	delete top(100) auditing.Revision
	from auditing.Revision r
	where not exists (	select 1 from auditing.PersonAbsence_AUD pab
						where pab.REV = r.id)
	and not exists (	select 1 from auditing.PersonAssignment_AUD pa
						where pa.REV = r.id)

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

/*
exec Purge
*/

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

if datediff(second,@start,getdate()) > @timeout 
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

if datediff(second,@start,getdate()) > @timeout 
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

if datediff(second,@start,getdate()) > @timeout 
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

if datediff(second,@start,getdate()) > @timeout 
	return

--Autodeny requests if not handled in time.
select @KeepUntil = dateadd(day,-1*(select isnull(Value,120) from PurgeSetting where [Key] = 'DenyPendingRequestsAfterNDays'),getdate())

update PersonRequest
   set RequestStatus = 1 -- Denied
     , DenyReason = 'RequestDenyReasonAutoPurge'
     , UpdatedOn = GETUTCDATE()
     , UpdatedBy = '3F0886AB-7B25-4E95-856A-0D726EDC2A67'  -- System
     , [Version] = [Version] + 1
  from PersonRequest pr inner join Request r on r.Parent = pr.Id
 where r.EndDateTime < @KeepUntil
   and pr.RequestStatus = 0 --Pending
   and pr.IsDeleted = 0

if datediff(second,@start,getdate()) > @timeout 
	return

--New Adherence read models. Purge for now since we have not yet built or tested with lots of historical data.
--Smaller chunks, it may time out on cloud otherwise.
select @TheDate = isnull(min(BelongsToDate),'20000101') from ReadModel.AdherencePercentage

while @TheDate < dateadd(day,-3,getdate())
begin
	delete ReadModel.AdherencePercentage
	where BelongsToDate <= @TheDate

	select @TheDate = dateadd(day,1,@TheDate)

	if datediff(second,@start,getdate()) > @timeout 
		return
end

--schedule related read models
select @KeepUntil = DATEADD(day, -1*(select isnull(Value, 30) from PurgeSetting where [Key] = 'DaysToKeepReadmodels'), GETDATE())

delete ReadModel.PersonScheduleDay
where BelongsToDate < @KeepUntil

delete ReadModel.ScheduleDay
where BelongsToDate < @KeepUntil

delete ReadModel.ScheduleProjectionReadOnly
where BelongsToDate < @KeepUntil

if datediff(second,@start,getdate()) > @timeout 
	return

--delete empty schedule read model records
delete ReadModel.ScheduleDay
where NotScheduled = 1

delete ReadModel.PersonScheduleDay
where Start is NULL and [End] is NULL

--Data from DELETED Scenarios
--Schedules
set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) PersonAssignment
	from PersonAssignment pa
	inner join Scenario s on s.id = pa.Scenario and s.IsDeleted = 1 and s.DefaultScenario <> 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) PersonAbsence
	from PersonAbsence pa
	inner join Scenario s on s.id = pa.Scenario and s.IsDeleted = 1 and s.DefaultScenario <> 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) AgentDayScheduleTag
	from AgentDayScheduleTag pa
	inner join Scenario s on s.id = pa.Scenario and s.IsDeleted = 1 and s.DefaultScenario <> 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) Note
	from Note pa
	inner join Scenario s on s.id = pa.Scenario and s.IsDeleted = 1 and s.DefaultScenario <> 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) PublicNote
	from PublicNote pa
	inner join Scenario s on s.id = pa.Scenario and s.IsDeleted = 1 and s.DefaultScenario <> 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

set @RowCount = 1
while @RowCount > 0
begin
	delete top (1000) Note
	from Note pa
	inner join Scenario s on s.id = pa.Scenario and s.IsDeleted = 1 and s.DefaultScenario <> 1

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

--delete job result artifacts according to purge setting
select @KeepUntil = DATEADD(day, -1*(select isnull(Value, 30) from PurgeSetting where [Key] = 'DaysToKeepJobResultArtifacts'), GETDATE())
CREATE TABLE #ExpiredArtifacts (Id uniqueidentifier NOT NULL, Parent uniqueidentifier NOT NULL)

set @RowCount = 1
while @RowCount > 0
begin

	Insert Into #ExpiredArtifacts select  top 1000 Id, Parent from dbo.JobResultArtifact
		where CreateTime < @KeepUntil
	delete from dbo.JobResultArtifact where id in (select id from #ExpiredArtifacts)
	delete from dbo.JobResultDetail where Parent in (select distinct Parent from #ExpiredArtifacts)
	delete from dbo.JobResult where id in (select distinct Parent from #ExpiredArtifacts)
	truncate table #ExpiredArtifacts

	select @RowCount = @@rowcount
	if datediff(second,@start,getdate()) > @timeout 
		return
end

--ToDo: BudgetDay, Meeting, MultiSiteDay, SkillDay
/*
exec Purge
*/

END

GO

