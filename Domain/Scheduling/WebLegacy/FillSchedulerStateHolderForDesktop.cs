using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderForDesktop : FillSchedulerStateHolder
	{
		private readonly DesktopContext _desktopContext;

		public FillSchedulerStateHolderForDesktop(DesktopContext desktopContext, PersonalSkillsProvider personalSkillsProvider) : base(personalSkillsProvider)
		{
			_desktopContext = desktopContext;
		}

		protected override IScenario FetchScenario()
		{
			return _desktopContext.CurrentContext().SchedulerStateHolderFrom.Schedules.Scenario;
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization = stateHolderFrom.SchedulingResultState.PersonsInOrganization.Where(x => agentIds.Contains(x.Id.Value)).ToList();
			schedulerStateHolderTo.AllPermittedPersons.Clear();
			stateHolderFrom.AllPermittedPersons.Where(x => agentIds.Contains(x.Id.Value))
				.ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			schedulerStateHolderTo.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>(stateHolderFrom.SchedulingResultState.SkillDays);
			schedulerStateHolderTo.SchedulingResultState.AddSkills(skills.ToArray());
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			var scheduleDictionary = new ScheduleDictionary(scenario, stateHolderFrom.Schedules.Period, new PersistableScheduleDataPermissionChecker());
			using (TurnoffPermissionScope.For(scheduleDictionary))
			{
				moveSchedules(stateHolderFrom.Schedules, scheduleDictionary, agents,
					stateHolderFrom.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(stateHolderFrom.TimeZoneInfo));
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

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.RequestedPeriod = _desktopContext.CurrentContext().SchedulerStateHolderFrom.RequestedPeriod;
			schedulerStateHolderTo.ConsiderShortBreaks = false; //TODO check if this is the wanted behaviour in other cases than intraday optimization
		}

		private static void moveSchedules(IScheduleDictionary fromDic,
			IScheduleDictionary toDic,
			IEnumerable<IPerson> agents,
			DateOnlyPeriod period)
		{
			foreach (var agent in agents)
			{
				var fromScheduleDays = fromDic[agent].ScheduledDayCollection(period);
				foreach (var fromScheduleDay in fromScheduleDays)
				{
					var toScheduleDay = toDic[agent].ScheduledDay(fromScheduleDay.DateOnlyAsPeriod.DateOnly);
					fromScheduleDay.PersistableScheduleDataCollection().OfType<IPersonAssignment>().ForEach(x => toScheduleDay.Add(x));
					fromScheduleDay.PersistableScheduleDataCollection().OfType<IPersonAbsence>().ForEach(x => toScheduleDay.Add(x));
					fromScheduleDay.PersonMeetingCollection().ForEach(x => ((ScheduleRange)toDic[agent]).Add(x));
					fromScheduleDay.PersonRestrictionCollection().ForEach(x => ((ScheduleRange)toDic[agent]).Add(x));
					fromScheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>().ForEach(x => toScheduleDay.Add(x));

					toDic.Modify(ScheduleModifier.Scheduler, toScheduleDay, NewBusinessRuleCollection.Minimum(),
						new DoNothingScheduleDayChangeCallBack(), new ScheduleTagSetter(fromScheduleDay.ScheduleTag()));
				}
			}
		}
	}
}