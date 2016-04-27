using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
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
		private readonly IResourceOptimizationHelper _resoureOptimizationHelper;

		public ScheduleOvertimeOnNonScheduleDays(Func<ISchedulerStateHolder> schedulerStateHolder,
			ITeamBlockScheduler teamBlockScheduler,
			IMatrixListFactory matrixListFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamInfoFactory teamInfoFactory,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IResourceOptimizationHelper resoureOptimizationHelper)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockScheduler = teamBlockScheduler;
			_matrixListFactory = matrixListFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamInfoFactory = teamInfoFactory;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_resoureOptimizationHelper = resoureOptimizationHelper;
		}

		public void SchedulePersonOnDay(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			if (overtimePreferences.ShiftBagOvertimeScheduling == null)
				return;

			var stateHolder = _schedulerStateHolder();
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var agent = scheduleDay.Person;
			_groupPersonBuilderWrapper.SetSingleAgentTeam();

			//REMOVE
			var resDelayerREMOVEME = new ResourceCalculateDelayer(_resoureOptimizationHelper, 1, true);

			//TODO? reuse? slow?
			var matrixes = _matrixListFactory.CreateMatrixListForSelection(new[]{scheduleDay});

			var teamInfo = _teamInfoFactory.CreateTeamInfo(agent, date, matrixes);
			var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, date, BlockFinderType.SingleDay, true);
			//TODO reuse? slow?
			var rollbackService =
				new SchedulePartModifyAndRollbackService(
					stateHolder.SchedulingResultState,
					new DoNothingScheduleDayChangeCallBack(), 
					new ScheduleTagSetter(overtimePreferences.ScheduleTag));

			//TODO ???
			var schedulingOptions = new SchedulingOptions();


			//TODO: flytta in shiftnudgedirective till metoden istället?
			_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, date, schedulingOptions, rollbackService, resDelayerREMOVEME,
				stateHolder.SchedulingResultState, new ShiftNudgeDirective());


			//hackeri hackera
			var currScheduleDay = stateHolder.Schedules[agent].ScheduledDay(date);
			var currLayers = currScheduleDay.PersonAssignment().MainActivities();
			currScheduleDay.Clear<IPersonAssignment>();
			//fix multiplicatorset
			currLayers.ForEach(x => currScheduleDay.CreateAndAddOvertime(x.Payload, x.Period, new MultiplicatorDefinitionSet("_", MultiplicatorType.OBTime)));
			stateHolder.Schedules.Modify(currScheduleDay);

		}
	}
}