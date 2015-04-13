using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
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
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;

		public TeamBlockMoveTimeOptimizer(ISchedulingOptionsCreator schedulingOptionsCreator,
			ITeamBlockMoveTimeDescisionMaker decisionMaker, ITeamBlockClearer teamBlockClearer,
			ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockScheduler teamBlockScheduler,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation)
		{
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_decisionMaker = decisionMaker;
			_teamBlockClearer = teamBlockClearer;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockScheduler = teamBlockScheduler;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
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

			TimeSpan totalContractTimeBefore = firstDayContractTime.Add(secondDayContractTime);

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
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				return false;
			}

			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;
			success = _teamBlockScheduler.ScheduleTeamBlockDay(secondTeamBlock, secondDayDate, schedulingOptions,
				rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, shiftNudgeDirective);

			if (!success)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
				return false;
			}

			//IScheduleDayPro firstDay1 = matrix.GetScheduleDayByKey(daysToBeMoved[0]);
			//IScheduleDay firstScheduleDay1 = firstDay1.DaySchedulePart();
			//TimeSpan firstDayContractTime1 = firstScheduleDay1.ProjectionService().CreateProjection().ContractTime();

			//IScheduleDayPro secondDay1 = matrix.GetScheduleDayByKey(daysToBeMoved[1]);
			//IScheduleDay secondScheduleDay1 = secondDay1.DaySchedulePart();
			//TimeSpan secondDayContractTime1 = secondScheduleDay1.ProjectionService().CreateProjection().ContractTime();

			//TimeSpan totalContractTimeAfter = firstDayContractTime1.Add(secondDayContractTime1);
			//if (totalContractTimeBefore != totalContractTimeAfter)
			//{
			//	_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
			//	return false;
			//}


			double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			bool isPeriodBetter = newPeriodValue < oldPeriodValue;
			if (!isPeriodBetter)
			{
				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);
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
