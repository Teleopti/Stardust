using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolderForDesktop : FillSchedulerStateHolder
	{
		private readonly DesktopContext _desktopContext;
		private readonly ReducedSkillsProvider _reducedSkillsProvider;
		private readonly AddReducedSkillDaysToStateHolder _addReducedSkillDaysToStateHolder;
		private readonly ICurrentAuthorization _currentAuthorization;

		public FillSchedulerStateHolderForDesktop(DesktopContext desktopContext, PersonalSkillsProvider personalSkillsProvider, ReducedSkillsProvider reducedSkillsProvider, AddReducedSkillDaysToStateHolder addReducedSkillDaysToStateHolder, ICurrentAuthorization currentAuthorization) : base(personalSkillsProvider)
		{
			_desktopContext = desktopContext;
			_reducedSkillsProvider = reducedSkillsProvider;
			_addReducedSkillDaysToStateHolder = addReducedSkillDaysToStateHolder;
			_currentAuthorization = currentAuthorization;
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
			var skillDaysContainingReducedSkills = _desktopContext.CurrentContext().SchedulerStateHolderFrom.SchedulingResultState.SkillDays;
			var reducedSkills = _reducedSkillsProvider.Execute(schedulerStateHolderTo, period);

			_addReducedSkillDaysToStateHolder.Execute(schedulerStateHolderTo, period, reducedSkills, skillDaysContainingReducedSkills);
		}

		protected override void FillBpos(ISchedulerStateHolder schedulerStateHolderTo, IEnumerable<ISkill> skills, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			schedulerStateHolderTo.SchedulingResultState.ExternalStaff = stateHolderFrom.SchedulingResultState.ExternalStaff;
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var stateHolderFrom = _desktopContext.CurrentContext().SchedulerStateHolderFrom;
			var scheduleDictionary = new ScheduleDictionary(scenario, stateHolderFrom.Schedules.Period, new PersistableScheduleDataPermissionChecker(_currentAuthorization), _currentAuthorization);
			using (TurnoffPermissionScope.For(scheduleDictionary))
			{
				moveSchedules(stateHolderFrom, scheduleDictionary, agents);
			}
			schedulerStateHolderTo.SchedulingResultState.Schedules = scheduleDictionary;
			scheduleDictionary.TakeSnapshot();
		}

		protected override void PreFill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			var currContext = _desktopContext.CurrentContext();
			schedulerStateHolderTo.SchedulingResultState.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
			var dayOffTemplate = currContext.SchedulerStateHolderFrom.CommonStateHolder.DayOffs.NonDeleted().First();
			schedulerStateHolderTo.CommonStateHolder.SetDayOffTemplate(dayOffTemplate);
			schedulerStateHolderTo.RequestedPeriod = currContext.SchedulerStateHolderFrom.RequestedPeriod;
			schedulerStateHolderTo.ConsiderShortBreaks = false; //TODO check if this is the wanted behaviour in other cases than intraday optimization
		}

		private static void moveSchedules(ISchedulerStateHolder schedulerStateHolderFrom,
			IScheduleDictionary toDic,
			IEnumerable<IPerson> agents)
		{
			foreach (var fromScheduleRange in agents.Select(agent => schedulerStateHolderFrom.Schedules[agent]))
			{
				//can probably optimize and not copy all schedules here (using choosing Period instead)
				fromScheduleRange.CopyTo(toDic[fromScheduleRange.Person]);
			}
		}
	}
}