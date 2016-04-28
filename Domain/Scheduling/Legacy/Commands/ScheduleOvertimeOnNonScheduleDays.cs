using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleOvertimeOnNonScheduleDays
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;

		public ScheduleOvertimeOnNonScheduleDays(Func<ISchedulerStateHolder> schedulerStateHolder,
			ITeamBlockScheduler teamBlockScheduler,
			IMatrixListFactory matrixListFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamInfoFactory teamInfoFactory,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockScheduler = teamBlockScheduler;
			_matrixListFactory = matrixListFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamInfoFactory = teamInfoFactory;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
		}

		public void SchedulePersonOnDay(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var agent = scheduleDay.Person;
			if (jumpOutEarly(scheduleDay, overtimePreferences, agent, date))
				return;

			var stateHolder = _schedulerStateHolder();
			_groupPersonBuilderWrapper.SetSingleAgentTeam();

			//hackera hackeri
			var orgPersonAss = scheduleDay.PersonAssignment(true).Clone() as IPersonAssignment;
			var addDayOff = false;
			if (scheduleDay.HasDayOff())
			{
				scheduleDay.DeleteDayOff();
				stateHolder.Schedules.Modify(scheduleDay);
				addDayOff = true;
			}

			var matrixes = _matrixListFactory.CreateMatrixListForSelection(new[]{scheduleDay});
			var teamInfo = _teamInfoFactory.CreateTeamInfo(agent, date, matrixes);
			var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, date, BlockFinderType.SingleDay, true);
			var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new DoNothingScheduleDayChangeCallBack(), new ScheduleTagSetter(overtimePreferences.ScheduleTag));
			//TODO ???
			var schedulingOptions = new SchedulingOptions();
			_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, date, schedulingOptions, rollbackService, resourceCalculateDelayer, stateHolder.SchedulingResultState, new ShiftNudgeDirective());

			var rules = NewBusinessRuleCollection.Minimum();
			if (!overtimePreferences.AllowBreakMaxWorkPerWeek)
			{
				rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
			}

			//hackeri hackera
			var currScheduleDay = stateHolder.Schedules[agent].ScheduledDay(date);
			var currLayers = currScheduleDay.PersonAssignment().MainActivities();
			currScheduleDay.Clear<IPersonAssignment>();

			if (addDayOff)
				orgPersonAss.SetThisAssignmentsDayOffOn(currScheduleDay.PersonAssignment(true));

			currLayers.ForEach(x => currScheduleDay.CreateAndAddOvertime(x.Payload, x.Period, overtimePreferences.OvertimeType));

			//TODO: use correct tag setter
			rollbackService.ModifyStrictly(currScheduleDay, new ScheduleTagSetter(new ScheduleTag()), rules);
		}

		private static bool jumpOutEarly(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences, IPerson agent, DateOnly date)
		{
			if (!scheduleDay.PersonAssignment(true).ShiftLayers.IsEmpty())
				return true;

			if (!scheduleDay.PersonAbsenceCollection().IsEmpty())
				return true;
			if (overtimePreferences.ShiftBagOvertimeScheduling == null)
				return true;

			var definitionSets = agent.Period(date)
				.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Where(
					x => x.MultiplicatorType == MultiplicatorType.Overtime);

			return !definitionSets.Contains(overtimePreferences.OvertimeType);
		}
	}
}