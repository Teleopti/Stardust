using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
			var scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
			var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new DoNothingScheduleDayChangeCallBack(), scheduleTagSetter);
			using (OvertimeSchedulingScope.Set(stateHolder.Schedules, scheduleDay, overtimePreferences, scheduleTagSetter, rollbackService))
			{
				var matrixes = _matrixListFactory.CreateMatrixListForSelection(new[] { scheduleDay });
				var teamInfo = _teamInfoFactory.CreateTeamInfo(agent, date, matrixes);
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, date, BlockFinderType.SingleDay, true);

				var schedulingOptions = new SchedulingOptions();	//TODO Is this even needed?
				_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, date, schedulingOptions, rollbackService, resourceCalculateDelayer, stateHolder.SchedulingResultState, new ShiftNudgeDirective());
			}
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