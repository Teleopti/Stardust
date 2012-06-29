/****** Object:  StoredProcedure [dbo].[Purge]    Script Date: 02/03/2012 17:21:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Purge]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Purge]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<AF>
-- Create date: <2012-02-02>
-- Description:	<Purge old data from app db>
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
declare @BatchSize int
declare @MaxDate datetime

/*
exec Purge
*/

select @ForecastKeepYears = isnull(KeepYears,100) from PurgeSetting where [Key] = 'Forecast'
select @ForecastsKeepUntil = dateadd(year,-1*@ForecastKeepYears,getdate())
select @ScheduleKeepYears = isnull(KeepYears,100) from PurgeSetting where [Key] = 'Schedule'
select @ScheduleKeepUntil = dateadd(year,-1*@ScheduleKeepYears,getdate())
select @MessageKeepYears = isnull(KeepYears,100) from PurgeSetting where [Key] = 'Message'
select @MessageKeepUntil = dateadd(year,-1*@MessageKeepYears,getdate())
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
select @MaxDate = dateadd(day,@BatchSize,isnull(min(Minimum),'19900101')) from PersonAssignment

delete PersonAssignment --Lovely, cascade delete is on...
from PersonAssignment pa
where 1 = 1
and pa.Minimum < @ScheduleKeepUntil
and pa.Minimum < @MaxDate

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


END

GO


