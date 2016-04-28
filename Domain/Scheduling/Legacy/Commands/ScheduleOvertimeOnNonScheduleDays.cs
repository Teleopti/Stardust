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
			if (!scheduleDay.PersonAssignment(true).ShiftLayers.IsEmpty())
				return;

			if(!scheduleDay.PersonAbsenceCollection().IsEmpty())
				return;

			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var agent = scheduleDay.Person;
			if (overtimePreferences.ShiftBagOvertimeScheduling == null)
				return;
			
			var definitionSets = agent.Period(date)
					.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Where(
						x => x.MultiplicatorType == MultiplicatorType.Overtime);

			if(!definitionSets.Contains(overtimePreferences.OvertimeType))
				return;

			var stateHolder = _schedulerStateHolder();
			_groupPersonBuilderWrapper.SetSingleAgentTeam();

			//TODO? reuse? slow?

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

			//TODO reuse? slow?
			var rollbackService =
				new SchedulePartModifyAndRollbackService(
					stateHolder.SchedulingResultState,
					new DoNothingScheduleDayChangeCallBack(), 
					new ScheduleTagSetter(overtimePreferences.ScheduleTag));

			//TODO ???
			var schedulingOptions = new SchedulingOptions();


			//TODO: flytta in shiftnudgedirective till metoden istället?
			_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, date, schedulingOptions, rollbackService, resourceCalculateDelayer,
				stateHolder.SchedulingResultState, new ShiftNudgeDirective());


			//hackeri hackera
			var currScheduleDay = stateHolder.Schedules[agent].ScheduledDay(date);
			var currLayers = currScheduleDay.PersonAssignment().MainActivities();
			currScheduleDay.Clear<IPersonAssignment>();

			if (addDayOff)
				orgPersonAss.SetThisAssignmentsDayOffOn(currScheduleDay.PersonAssignment(true));

			currLayers.ForEach(x => currScheduleDay.CreateAndAddOvertime(x.Payload, x.Period, overtimePreferences.OvertimeType));
			stateHolder.Schedules.Modify(currScheduleDay);
		}
	}
}