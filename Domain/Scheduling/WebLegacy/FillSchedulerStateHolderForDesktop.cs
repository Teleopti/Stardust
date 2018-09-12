using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_ReducingSkillsDifferentOpeningHours_76176)]
	public class FillSchedulerStateHolderForDesktop : FillSchedulerStateHolderForDesktopOLD
	{
		private readonly DesktopContext _desktopContext;
		private readonly SkillsOnAgentsProvider _skillsOnAgentsProvider;

		public FillSchedulerStateHolderForDesktop(DesktopContext desktopContext, PersonalSkillsProvider personalSkillsProvider, SkillsOnAgentsProvider skillsOnAgentsProvider) 
			: base(desktopContext, personalSkillsProvider)
		{
			_desktopContext = desktopContext;
			_skillsOnAgentsProvider = skillsOnAgentsProvider;
		}
		
		protected override void AddSkillDaysForReducedSkills(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			var orgSkillDays = _desktopContext.CurrentContext().SchedulerStateHolderFrom.SchedulingResultState.SkillDays;
			var agentSkills = _skillsOnAgentsProvider.Execute(schedulerStateHolderTo.SchedulingResultState.LoadedAgents, period); //can be optimized to only selected agents
			foreach (var reducedSkill in agentSkills.Except(schedulerStateHolderTo.SchedulingResultState.Skills))
			{
				if (orgSkillDays.TryGetValue(reducedSkill, out var reducedSkillDays))
				{
					var newSkillDays = new List<ISkillDay>();
					foreach (var skillDay in reducedSkillDays)
					{
						var skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(), skillDay.CurrentDate.ToDateTimePeriod(reducedSkill.TimeZone))
							{ManualAgents = 0};
						var newSkillDay = new SkillDay(skillDay.CurrentDate, reducedSkill, skillDay.Scenario, skillDay.WorkloadDayCollection, new List<ISkillDataPeriod> {skillDataPeriod});
						newSkillDays.Add(newSkillDay);
					}

					var skillDayCalculator = new SkillDayCalculator(reducedSkill, newSkillDays, period);
					foreach (var newSkillDay in newSkillDays)
					{
						newSkillDay.SkillDayCalculator = skillDayCalculator;
					}
					schedulerStateHolderTo.SchedulingResultState.SkillDays[reducedSkill] = newSkillDays;
				}
			}
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

		protected override void AddSkillDaysForReducedSkills(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
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

		private readonly object moveSchedulesLock = new object();
		private void moveSchedules(ISchedulerStateHolder schedulerStateHolderFrom,
			IScheduleDictionary toDic,
			IEnumerable<IPerson> agents)
		{
			DateOnlyPeriod period;
			var fromScheduleRanges = new List<IScheduleRange>();
			lock (moveSchedulesLock)
			{
				period = schedulerStateHolderFrom.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(schedulerStateHolderFrom.TimeZoneInfo);
				fromScheduleRanges.AddRange(agents.Select(agent => schedulerStateHolderFrom.Schedules[agent]));
			}

			foreach (var fromScheduleRange in fromScheduleRanges)
			{
				fromScheduleRange.CopyTo(toDic[fromScheduleRange.Person], period);
			}
		}
	}
}