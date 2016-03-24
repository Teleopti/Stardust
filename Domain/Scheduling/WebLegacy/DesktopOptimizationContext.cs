using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopOptimizationContext : FillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization
	{
		private ISchedulerStateHolder _schedulerStateHolderFrom;
		private IOptimizationPreferences _optimizationPreferences;

		protected override IScenario FetchScenario()
		{
			return _schedulerStateHolderFrom.Schedules.Scenario;
		}

		protected override void FillAgents(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<Guid> agentIds, DateOnlyPeriod period)
		{
			_schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization.Where(x => agentIds.Contains(x.Id.Value))
							.ForEach(x => schedulerStateHolderTo.SchedulingResultState.PersonsInOrganization.Add(x));
			_schedulerStateHolderFrom.AllPermittedPersons.Where(x => agentIds.Contains(x.Id.Value)).ForEach(x => schedulerStateHolderTo.AllPermittedPersons.Add(x));
		}

		protected override void FillSkillDays(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>(_schedulerStateHolderFrom.SchedulingResultState.SkillDays);
			schedulerStateHolderTo.SchedulingResultState.AddSkills(skills.ToArray());
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var scheduleDictionary = new ScheduleDictionary(scenario, _schedulerStateHolderFrom.Schedules.Period);
			using (TurnoffPermissionScope.For(scheduleDictionary))
			{
				moveSchedules(_schedulerStateHolderFrom.Schedules, scheduleDictionary, schedulerStateHolderTo.AllPermittedPersons,
					_schedulerStateHolderFrom.Schedules.Period.LoadedPeriod().ToDateOnlyPeriod(_schedulerStateHolderFrom.TimeZoneInfo));
			}
			schedulerStateHolderTo.SchedulingResultState.Schedules = scheduleDictionary;
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
		}

		protected override void PostFill(ISchedulerStateHolder schedulerStateHolder, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			schedulerStateHolder.RequestedPeriod = _schedulerStateHolderFrom.RequestedPeriod;
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			var agentsToMove = modifiedScheduleDictionary.Keys;
			moveSchedules(modifiedScheduleDictionary, _schedulerStateHolderFrom.Schedules, agentsToMove, period);
		}

		public IDisposable Add(ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences)
		{
			_schedulerStateHolderFrom = schedulerStateHolderFrom;
			_optimizationPreferences = optimizationPreferences;
			return new GenericDisposable(() =>
			{
				_schedulerStateHolderFrom = null;
				_optimizationPreferences = null;
			});
		}

		private static void moveSchedules(IScheduleDictionary fromDic, IScheduleDictionary toDic, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			foreach (var agent in agents)
			{
				var fromScheduleDays = fromDic[agent].ScheduledDayCollection(period);
				foreach (var fromScheduleDay in fromScheduleDays)
				{
					var toScheduleDay = toDic[agent].ScheduledDay(fromScheduleDay.DateOnlyAsPeriod.DateOnly);	
					var toAssignment = toScheduleDay.PersonAssignment(true);
					toAssignment.FillWithDataFrom(fromScheduleDay.PersonAssignment(true));

					fromScheduleDay.PersistableScheduleDataCollection().OfType<IPersonAbsence>().ForEach(x => toScheduleDay.Add(x));
					fromScheduleDay.PersonMeetingCollection().ForEach(x => ((ScheduleRange)toDic[agent]).Add(x));
					fromScheduleDay.PersonRestrictionCollection().ForEach(x => ((ScheduleRange) toDic[agent]).Add(x));
					fromScheduleDay.PersistableScheduleDataCollection().OfType<IPreferenceDay>().ForEach(x => toScheduleDay.Add(x));

					toDic.Modify(toScheduleDay);
				}
			}
		}

		public IOptimizationPreferences Fetch()
		{
			return _optimizationPreferences;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			return _schedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization;
		}
	}
}