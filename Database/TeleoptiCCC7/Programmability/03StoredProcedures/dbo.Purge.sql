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
declare @ForecastKeepYears int
declare @ForecastsKeepUntil datetime
declare @ScheduleKeepYears int
declare @ScheduleKeepUntil datetime
declare @MessageKeepYears int
declare @MessageKeepUntil datetime
declare @PayrollKeepYears int
declare @PayrollKeepUntil datetime
declare @SecurityAuditKeepDays int
declare @SecurityAuditKeepUntil datetime
declare @RequestsKeepMonths int
declare @RequestsKeepUntil datetime

declare @BatchSize int
declare @MaxDate datetime

/*
exec Purge
*/

--Get values
select @ForecastKeepYears = isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepForecast'
select @ScheduleKeepYears = isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepSchedule'
select @MessageKeepYears = isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepMessage'
select @PayrollKeepYears = isnull(Value,100) from PurgeSetting where [Key] = 'YearsToKeepPayroll'
select @SecurityAuditKeepDays = isnull(Value,30) from PurgeSetting where [Key] = 'DaysToKeepSecurityAudit'
select @RequestsKeepMonths = isnull(Value,120) from PurgeSetting where [Key] = 'MonthsToKeepRequests'

--Create a KeepUntil
select @ForecastsKeepUntil = dateadd(year,-1*@ForecastKeepYears,getdate())
select @ScheduleKeepUntil = dateadd(year,-1*@ScheduleKeepYears,getdate())
select @MessageKeepUntil = dateadd(year,-1*@MessageKeepYears,getdate())
select @PayrollKeepUntil = dateadd(year,-1*@MessageKeepYears,getdate())
select @SecurityAuditKeepUntil = dateadd(day,-1*@SecurityAuditKeepDays,getdate())
select @RequestsKeepUntil = dateadd(month,-1*@RequestsKeepMonths,getdate())

select @BatchSize = 14

--Forecast
select @MaxDate = dateadd(day,@BatchSize,isnull(min(WorkloadDate),'19900101'))
from WorkloadDayBase wdb
inner join WorkloadDay wd on wdb.id = wd.WorkloadDayBase
where wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete TemplateTaskPeriod
from TemplateTaskPeriod ttp
inner join WorkloadDayBase wdb on ttp.Parent = wdb.Id
inner join WorkloadDay wd on wdb.id = wd.WorkloadDayBase
where 1=1
and wdb.WorkloadDate < @ForecastsKeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete WorkloadDay 
from WorkloadDay wd
inner join WorkloadDayBase wdb on wdb.id = wd.WorkloadDayBase
where 1=1
and wdb.WorkloadDate < @ForecastsKeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete OpenhourList
from OpenhourList ol
inner join WorkloadDayBase wdb on wdb.id = ol.Parent
where 1=1
and wdb.WorkloadDate < @ForecastsKeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates


delete WorkloadDayBase
from WorkloadDayBase wdb
where 1=1
and Not exists (select 1 from WorkloadDayTemplate wdt
				where wdt.WorkloadDayBase = wdb.Id) --At one customer I found templates on incorrect dates, avoid those and do not try to fix them here in the purge.
and wdb.WorkloadDate < @ForecastsKeepUntil
and wdb.WorkloadDate < @MaxDate
and wdb.WorkloadDate > '19600101' --Avoid hitting templates

delete SkillDataPeriod
from SkillDataPeriod sdp
inner join SkillDay sd on sdp.Parent = sd.Id
where 1=1
and sd.SkillDayDate < @ForecastsKeepUntil
and sd.SkillDayDate < @MaxDate

delete SkillDay
from SkillDay sd
where 1=1
and sd.SkillDayDate < @ForecastsKeepUntil
and sd.SkillDayDate < @MaxDate


--Schedule
select @MaxDate = dateadd(day,@BatchSize,isnull(min(Date),'19900101')) from PersonAssignment

delete PersonAssignment --Lovely, cascade delete is on...
from PersonAssignment pa
where 1 = 1
and pa.Date < @ScheduleKeepUntil
and pa.Date < @MaxDate

delete PersonAbsence --Lovely, cascade delete is on...
from PersonAbsence pa
where 1 = 1
and pa.Maximum < @ScheduleKeepUntil
and pa.Maximum < @MaxDate

delete Note
from Note n
where 1 = 1
and n.NoteDate < @ScheduleKeepUntil
and n.NoteDate < @MaxDate

delete AgentDayScheduleTag
from AgentDayScheduleTag ad
where 1 = 1
and ad.TagDate < @ScheduleKeepUntil
and ad.TagDate < @MaxDate

--Remove deleted skills from persons
delete PersonSkill
from PersonSkill ps
inner join Skill s on ps.Skill = s.Id
where s.IsDeleted = 1

--Remove roles on deleted persons to prevent them from showing up in Permissions
delete PersonInApplicationRole
from PersonInApplicationRole piar
inner join Person p on p.Id = piar.Person
where p.IsDeleted = 1

--Remove deleted budget groups from persons
update PersonPeriod set BudgetGroup = NULL
from PersonPeriod pp
inner join BudgetGroup bg on pp.BudgetGroup = bg.Id
where bg.IsDeleted = 1

--Messages
select @MaxDate = dateadd(day,@BatchSize,isnull(min(UpdatedOn),'19900101')) from PushMessageDialogue

delete DialogueMessage
from DialogueMessage dm
inner join PushMessageDialogue pmd on pmd.Id = dm.Parent
where pmd.UpdatedOn < @MessageKeepUntil
and pmd.UpdatedOn < @MaxDate

delete PushMessageDialogue
from PushMessageDialogue pmd
where pmd.UpdatedOn < @MessageKeepUntil
and pmd.UpdatedOn < @MaxDate

delete ReplyOptions
from ReplyOptions ro
inner join PushMessage pm on ro.id = pm.Id
where not exists (select 1 from PushMessageDialogue pmd where pmd.PushMessage = pm.Id)

delete PushMessage
from PushMessage pm
where not exists (select 1 from PushMessageDialogue pmd where pmd.PushMessage = pm.Id)

--Payroll
select @MaxDate = dateadd(day,@BatchSize,isnull(min(UpdatedOn),'19900101')) from PayrollResult

delete PayrollResultDetail
from PayrollResultDetail prd
inner join PayrollResult pr on prd.Parent = pr.Id
where pr.UpdatedOn < @PayrollKeepUntil
and pr.UpdatedOn < @Maxdate

delete PayrollResult
from PayrollResult pr
where pr.UpdatedOn < @PayrollKeepUntil
and pr.UpdatedOn < @Maxdate

--SecurityAudit
delete [Auditing].[Security]
where  [DateTimeUtc] < @SecurityAuditKeepUntil

--Requests
delete top (50000) ShiftTradeSwapDetail
from ShiftTradeSwapDetail a
inner join ShiftTradeRequest st on st.Request = a.Parent
inner join Request r on r.id = st.Request
where r.EndDateTime < @RequestsKeepUntil


delete top (50000) ShiftTradeRequest
from ShiftTradeRequest a
inner join Request r on r.id = a.Request
where r.EndDateTime < @RequestsKeepUntil
and not exists (
	select 1 from ShiftTradeSwapDetail stsd
	where stsd.Parent = a.Request)

delete top (50000) AbsenceRequest
from AbsenceRequest a
inner join Request r on r.id = a.Request
where r.EndDateTime < @RequestsKeepUntil

delete top (50000) TextRequest
from TextRequest a
inner join Request r on r.id = a.Request
where r.EndDateTime < @RequestsKeepUntil


delete top (50000) Request
from Request r
where r.EndDateTime < @RequestsKeepUntil
and not exists (
	select 1 from AbsenceRequest a
	where a.Request = r.Id)
and not exists (
	select 1 from ShiftTradeRequest b
	where b.Request = r.Id)
and not exists (
	select 1 from TextRequest c
	where c.Request = r.Id)


delete PersonRequest
from PersonRequest pr
where not exists (
select 1 from Request r
where r.Parent = pr.Id)

END

GO


