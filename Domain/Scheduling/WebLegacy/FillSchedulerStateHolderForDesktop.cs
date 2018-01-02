using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_XXL_47258)]
	public class FillSchedulerStateHolderForDesktop : FillSchedulerStateHolderForDesktopOLD
	{
		public FillSchedulerStateHolderForDesktop(DesktopContext desktopContext, PersonalSkillsProvider personalSkillsProvider) : base(desktopContext, personalSkillsProvider)
		{
		}

		protected override void moveSchedules(ISchedulerStateHolder schedulerStateHolderFrom, IScheduleDictionary toDic, IEnumerable<IPerson> agents)
		{
			DateOnlyPeriod period;
			var scheduleRanges = new List<IScheduleRange>();
			lock (moveSchedulesInOneThreadOnly)
			{
				period = schedulerStateHolderFrom.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(schedulerStateHolderFrom.TimeZoneInfo);
				scheduleRanges.AddRange(agents.Select(agent => schedulerStateHolderFrom.Schedules[agent]));
			}

			var toScheduleDays = new List<IScheduleDay>();
			foreach (ScheduleRange range in scheduleRanges)
			{
				foreach (var fromScheduleDay in range.ScheduledDayCollection(period))
				{
					var toScheduleDay = range.ScheduledDay(fromScheduleDay.DateOnlyAsPeriod.DateOnly, true);
					var persistableScheduleDataCollection = fromScheduleDay.PersistableScheduleDataCollection();
					persistableScheduleDataCollection.OfType<IPersonAssignment>().ForEach(x => toScheduleDay.Add(x));
					persistableScheduleDataCollection.OfType<IPersonAbsence>().ForEach(x => toScheduleDay.Add(x));
					fromScheduleDay.PersonMeetingCollection().ForEach(x => range.Add(x));
					fromScheduleDay.PersonRestrictionCollection().ForEach(x => range.Add(x));
					persistableScheduleDataCollection.OfType<IPreferenceDay>().ForEach(x => toScheduleDay.Add(x));
					persistableScheduleDataCollection.OfType<IAgentDayScheduleTag>().ForEach(x => toScheduleDay.Add(x));
					persistableScheduleDataCollection.OfType<IStudentAvailabilityDay>().ForEach(x => toScheduleDay.Add(x));
					toScheduleDays.Add(toScheduleDay);
				}
			}
			toDic.Modify(ScheduleModifier.Scheduler, toScheduleDays, NewBusinessRuleCollection.Minimum(), new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter(), true);	
		}
	}
	
	public class FillSchedulerStateHolderForDesktopOLD : FillSchedulerStateHolder
	{
		private readonly DesktopContext _desktopContext;

		public FillSchedulerStateHolderForDesktopOLD(DesktopContext desktopContext, PersonalSkillsProvider personalSkillsProvider) : base(personalSkillsProvider)
		{
			_desktopContext = desktopContext;
		}

		protected override void FillScenario(ISchedulerStateHolder schedulerStateHolderTo)
		{
			schedulerStateHolderTo.SetRequestedScenario(_desktopContext.CurrentContext().SchedulerStateHolderFrom.Schedules.Scenario);
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			schedulerStateHolderTo.SchedulingResultState.LoadedAgents = stateHolderFrom.SchedulingResultState.LoadedAgents.Where(x => agentIds.Contains(x.Id.Value)).ToList();
			schedulerStateHolderTo.ChoosenAgents.Clear();
			stateHolderFrom.ChoosenAgents.Where(x => agentIds.Contains(x.Id.Value))
				.ForEach(x => schedulerStateHolderTo.ChoosenAgents.Add(x));
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			schedulerStateHolderTo.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>(stateHolderFrom.SchedulingResultState.SkillDays);
			schedulerStateHolderTo.SchedulingResultState.AddSkills(skills.ToArray());
		}

		protected override void FillBpos(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			schedulerStateHolderTo.SchedulingResultState.ExternalStaff = stateHolderFrom.SchedulingResultState.ExternalStaff;
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			var scheduleDictionary = new ScheduleDictionary(scenario, stateHolderFrom.Schedules.Period, new PersistableScheduleDataPermissionChecker());
			using (TurnoffPermissionScope.For(scheduleDictionary))
			{
				moveSchedules(stateHolderFrom, scheduleDictionary, agents);
			}
			schedulerStateHolderTo.SchedulingResultState.Schedules = scheduleDictionary;
			scheduleDictionary.TakeSnapshot();
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
			var dayOffTemplate = _desktopContext.CurrentContext().SchedulerStateHolderFrom.CommonStateHolder.ActiveDayOffs.First();
			schedulerStateHolderTo.CommonStateHolder.SetDayOffTemplate(dayOffTemplate);
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.RequestedPeriod = _desktopContext.CurrentContext().SchedulerStateHolderFrom.RequestedPeriod;
			schedulerStateHolderTo.ConsiderShortBreaks = false; //TODO check if this is the wanted behaviour in other cases than intraday optimization
		}

		[RemoveMeWithToggle("Make private", Toggles.ResourcePlanner_XXL_47258)]
		protected readonly object moveSchedulesInOneThreadOnly = new object();
		[RemoveMeWithToggle("Make private", Toggles.ResourcePlanner_XXL_47258)]
		protected virtual void moveSchedules(ISchedulerStateHolder schedulerStateHolderFrom,
			IScheduleDictionary toDic,
			IEnumerable<IPerson> agents)
		{
			lock (moveSchedulesInOneThreadOnly)
			{
				var fromDic = schedulerStateHolderFrom.Schedules;
				var period = schedulerStateHolderFrom.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(schedulerStateHolderFrom.TimeZoneInfo);
				var toScheduleDays = agents.SelectMany(agent =>
				{
					var fromScheduleDaysAgent = fromDic[agent].ScheduledDayCollection(period);
					return fromScheduleDaysAgent.Select(fromScheduleDay =>
					{
						var range = (ScheduleRange) toDic[agent];
						var toScheduleDay = range.ScheduledDay(fromScheduleDay.DateOnlyAsPeriod.DateOnly, true);
						var persistableScheduleDataCollection = fromScheduleDay.PersistableScheduleDataCollection();
						persistableScheduleDataCollection.OfType<IPersonAssignment>().ForEach(x => toScheduleDay.Add(x));
						persistableScheduleDataCollection.OfType<IPersonAbsence>().ForEach(x => toScheduleDay.Add(x));
						fromScheduleDay.PersonMeetingCollection().ForEach(x => range.Add(x));
						fromScheduleDay.PersonRestrictionCollection().ForEach(x => range.Add(x));
						persistableScheduleDataCollection.OfType<IPreferenceDay>().ForEach(x => toScheduleDay.Add(x));
						persistableScheduleDataCollection.OfType<IAgentDayScheduleTag>().ForEach(x => toScheduleDay.Add(x));
						persistableScheduleDataCollection.OfType<IStudentAvailabilityDay>().ForEach(x => toScheduleDay.Add(x));
						return toScheduleDay;
					});
				}).ToArray();
				//Maybe this should be done per agent instead? So far, testing has shown that fastest is to this with as many schedule days as possible though
				//Could it cause OOM ex in REALLY big dbs?
				toDic.Modify(ScheduleModifier.Scheduler, toScheduleDays, NewBusinessRuleCollection.Minimum(), new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter(), true);	
			}
		}
	}
}