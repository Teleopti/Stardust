using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeOptimizer
	{
		bool OptimizeTeam(IOptimizationPreferences optimizerPreferences, ITeamInfo teamInfo, IScheduleMatrixPro matrix, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class TeamBlockMoveTimeOptimizer : ITeamBlockMoveTimeOptimizer
	{
		private readonly ISchedulingOptionsCreator  _schedulingOptionsCreator;
		private readonly ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private readonly ITeamBlockClearer _teamBlockClearer;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamBlockScheduler _teamBlockScheduler;

		public TeamBlockMoveTimeOptimizer(ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockMoveTimeDescisionMaker decisionMaker, ITeamBlockClearer teamBlockClearer,
			ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockScheduler teamBlockScheduler)
		{
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_decisionMaker = decisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
		}

		public bool OptimizeTeam(IOptimizationPreferences optimizerPreferences, ITeamInfo teamInfo, IScheduleMatrixPro matrix, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			rollbackService.ClearModificationCollection();
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);

			double oldPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			IList<DateOnly> daysToBeMoved = _decisionMaker.Execute(matrix, optimizerPreferences);
			if (daysToBeMoved.Count == 0)
				return false;

			IScheduleDayPro firstDay = matrix.GetScheduleDayByKey(daysToBeMoved[0]);
			DateOnly firstDayDate = daysToBeMoved[0];
			IScheduleDay firstScheduleDay = firstDay.DaySchedulePart();
			TimeSpan firstDayContractTime = firstScheduleDay.ProjectionService().CreateProjection().ContractTime();

			IScheduleDayPro secondDay = matrix.GetScheduleDayByKey(daysToBeMoved[1]);
			DateOnly secondDayDate = daysToBeMoved[1];
			IScheduleDay secondScheduleDay = secondDay.DaySchedulePart();
			TimeSpan secondDayContractTime = secondScheduleDay.ProjectionService().CreateProjection().ContractTime();

			if (firstDayDate == secondDayDate)
				return false;

			if (firstDayContractTime > secondDayContractTime)
			{
				lockDay(teamInfo, secondDayDate);
				return true;
			}

			//delete schedule on the two days
			var firstTeamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, firstDayDate,
				schedulingOptions.BlockFinderTypeForAdvanceScheduling, false);
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, firstTeamBlock);

			var secondTeamBlock = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, secondDayDate,
				schedulingOptions.BlockFinderTypeForAdvanceScheduling, false);
			_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, secondTeamBlock);
																			
			var shiftNudgeDirective = new ShiftNudgeDirective();
			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;
			var success = _teamBlockScheduler.ScheduleTeamBlockDay(firstTeamBlock, firstDayDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);

			if (!success)
				return false;

			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;
			success = _teamBlockScheduler.ScheduleTeamBlockDay(secondTeamBlock, secondDayDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);

			if (!success)
				return false;

			double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			bool isPeriodBetter = newPeriodValue < oldPeriodValue;
			if (!isPeriodBetter)
			{
				rollbackService.Rollback();
				lockDay(teamInfo, firstDayDate);
				lockDay(teamInfo, secondDayDate);
				return true;
			}

			return true;
		}
	
		private void lockDay(ITeamInfo team, DateOnly day)
		{
			foreach (var matrix in team.MatrixesForGroupAndDate(day))
			{
				matrix.LockPeriod(new DateOnlyPeriod(day, day));
			}

		}

	}
}
