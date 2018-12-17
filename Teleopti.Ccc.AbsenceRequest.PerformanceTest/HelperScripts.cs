namespace Teleopti.Ccc.AbsenceRequest.PerformanceTest
{
	public static class HelperScripts
	{
		public static string ClearAbsenceRequestRequestPersonRequestOnPeriod => @"delete from AbsenceRequest
where request in (select id from request
where parent in (  select id from PersonRequest where Subject  = 'Story79139'))

delete from request
where parent in (  select id from PersonRequest where Subject  = 'Story79139')
 
delete from PersonRequest where Subject  = 'Story79139'";


		public static string ClearExistingAbsencesOnperiod => @"declare @start datetime = '2016-03-13 00:00:00' 
declare @end datetime = '2016-03-18 00:00:00' 
--select * 
delete from [QueuedAbsenceRequest]
delete 
from PersonAbsence
where [Minimum] between @start and @end or Maximum between @start and @end
DELETE FROM AbsenceRequest WHERE Request IN (SELECT Id from Request where StartDateTime between  @start  and @end)
delete from Request where StartDateTime between  @start  and @end and Id not in (select Request from ShiftTradeRequest)
delete from PersonRequest where id not in(select parent from Request )


truncate table [ReadModel].[SkillCombinationResource]
truncate table [ReadModel].[SkillCombination]
truncate table [ReadModel].[SkillCombinationResourceDelta]
truncate table [dbo].[JobStartTime]";

		public static string PersonWithValidSetupForIntradayRequestOnPeriod => @"

 update [AbsenceRequestOpenPeriod]
 set StaffingThresholdValidator = 3
  where Parent = 'E97BC114-8939-4A70-AE37-A338010FFF19'

select distinct top 400 p.Id 
from person p
inner join personperiod pp on pp.Parent = p.id
inner join PersonSkill ps on ps.Parent = pp.id
inner join Team t on t.id = pp.team
inner join [Site] s on t.Site = s.Id
inner join PersonAssignment pa
on pa.Person = p.Id
where pp.StartDate < '2016-03-18 00:00:00' and pp.EndDate > '2016-03-13 00:00:00' 
and p.WorkflowControlSet = 'E97BC114-8939-4A70-AE37-A338010FFF19'
and s.BusinessUnit = '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
and pa.Date between '2016-03-13 00:00:00'  and '2016-03-18 00:00:00'
and p.Id not in (select person from PersonAbsence where  '2016-03-16 00:00:00' between  DATEADD(day,-1,Minimum)  and  DATEADD(day,1,maximum) )";

		public static string SetBudgetGroupValidationOnWorkflowControlSet =
			@"update[AbsenceRequestOpenPeriod]
		set StaffingThresholdValidator = 3
		where Parent = 'E97BC114-8939-4A70-AE37-A338010FFF19'";

		public static string PersonWithValidSetupForIntradayRequestOnPeriodForParallelTests => @"

 update [AbsenceRequestOpenPeriod]
 set StaffingThresholdValidator = 1
  where Parent = 'E97BC114-8939-4A70-AE37-A338010FFF19'

select distinct top 200 p.Id 
from person p
inner join personperiod pp on pp.Parent = p.id
inner join PersonSkill ps on ps.Parent = pp.id
inner join Team t on t.id = pp.team
inner join [Site] s on t.Site = s.Id
inner join PersonAssignment pa
on pa.Person = p.Id
where pp.StartDate < '2016-03-18 00:00:00' and pp.EndDate > '2016-03-13 00:00:00' 
and p.WorkflowControlSet = 'E97BC114-8939-4A70-AE37-A338010FFF19'
and s.BusinessUnit = '1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B'
and pa.Date between '2016-03-13 00:00:00'  and '2016-03-18 00:00:00'
and p.Id not in (select person from PersonAbsence where  '2016-03-16 00:00:00' between  DATEADD(day,-1,Minimum)  and  DATEADD(day,1,maximum) )";

	}
}