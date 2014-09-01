using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeOptimizer
	{
		bool OptimizeMatrix(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> matrixesOnSelectedperiod);
	}

	public class TeamBlockMoveTimeOptimizer : ITeamBlockMoveTimeOptimizer
	{
		private readonly ISchedulingOptionsCreator  _schedulingOptionsCreator;
		private readonly ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IConstructAndScheduleSingleDayTeamBlock _constructAndScheduleSingleDayTeamBlock;
		private readonly IDeleteSelectedDaysForTeam _deleteSelectedDaysForTeam;

		public TeamBlockMoveTimeOptimizer(ISchedulingOptionsCreator schedulingOptionsCreator,ITeamBlockMoveTimeDescisionMaker decisionMaker, IResourceOptimizationHelper resourceOptimizationHelper,  IConstructAndScheduleSingleDayTeamBlock constructAndScheduleSingleDayTeamBlock, IDeleteSelectedDaysForTeam deleteSelectedDaysForTeam)
		{
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_decisionMaker = decisionMaker;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_constructAndScheduleSingleDayTeamBlock = constructAndScheduleSingleDayTeamBlock;
			_deleteSelectedDaysForTeam = deleteSelectedDaysForTeam;
		}
		public bool OptimizeMatrix(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> matrixesOnSelectedperiod)
		{

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);

			double oldPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			IList<DateOnly> daysToBeMoved = _decisionMaker.Execute(matrix, optimizerPreferences);
			if (daysToBeMoved.Count == 0)
				return false;
			rollbackService.ClearModificationCollection();
			IScheduleDayPro firstDay = matrix.GetScheduleDayByKey(daysToBeMoved[0]);
			DateOnly firstDayDate = daysToBeMoved[0];
			IScheduleDay firstScheduleDay = firstDay.DaySchedulePart();
			var originalFirstScheduleDay = (IScheduleDay)firstScheduleDay.Clone();
			TimeSpan firstDayContractTime = firstScheduleDay.ProjectionService().CreateProjection().ContractTime();

			IScheduleDayPro secondDay = matrix.GetScheduleDayByKey(daysToBeMoved[1]);
			DateOnly secondDayDate = daysToBeMoved[1];
			IScheduleDay secondScheduleDay = secondDay.DaySchedulePart();
			var originalSecondScheduleDay = (IScheduleDay)secondScheduleDay.Clone();
			TimeSpan secondDayContractTime = secondScheduleDay.ProjectionService().CreateProjection().ContractTime();

			if (firstDayDate == secondDayDate)
				return false;

			if (firstDayContractTime > secondDayContractTime)
			{
				lockDay(matrix, secondDayDate);
				return true;
			}

			//delete schedule on the two days
			_deleteSelectedDaysForTeam.PerformDelete( matrixesOnSelectedperiod, firstDayDate, secondDayDate, rollbackService,schedulingOptions.ConsiderShortBreaks);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																							schedulingOptions.ConsiderShortBreaks);
			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;
			var matrixForTeamOnGivenDate =
				matrixesOnSelectedperiod.Where(s => s.SchedulePeriod.DateOnlyPeriod.DayCollection().Contains(firstDayDate )); 
			if (!scheduleTeamBlock(matrixList, firstDayDate,matrixForTeamOnGivenDate, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, originalFirstScheduleDay, optimizerPreferences))
				return false;
			schedulingOptions.WorkShiftLengthHintOption  =  WorkShiftLengthHintOption.Free;
			matrixForTeamOnGivenDate =
				matrixesOnSelectedperiod.Where(s => s.SchedulePeriod.DateOnlyPeriod.DayCollection().Contains(secondDayDate)); 
			if (!scheduleTeamBlock(matrixList, secondDayDate,matrixForTeamOnGivenDate, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, originalSecondScheduleDay, optimizerPreferences))
				return false;


			double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			bool isPeriodBetter = newPeriodValue < oldPeriodValue;
			if (!isPeriodBetter)
			{
				rollbackService.Rollback();
				lockDay(matrix, firstDayDate);
				lockDay(matrix, secondDayDate);
				return true;
			}

			return true;
		}
		
		private void safeCalculateDate(DateOnly dayDate, IScheduleDay originalScheduleDay, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			resourceCalculateDelayer.CalculateIfNeeded(dayDate, originalScheduleDay.ProjectionService().CreateProjection().Period());
		}

		private bool scheduleTeamBlock(IList<IScheduleMatrixPro> matrixList, DateOnly dayDate, IEnumerable<IScheduleMatrixPro> forTeamOnGivenDate, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleDay originalScheduleDay, IOptimizationPreferences optimizationPreferences)
		{

			foreach (var matrix in forTeamOnGivenDate)
			{
				if (!_constructAndScheduleSingleDayTeamBlock.Schedule(matrixList, dayDate, matrix, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, optimizationPreferences))
				{
					rollbackService.Rollback();
					safeCalculateDate(dayDate, originalScheduleDay, resourceCalculateDelayer);
					return false;
				}
			}
			return true;
		}

		
		private void lockDay(IScheduleMatrixPro matrix, DateOnly day)
		{
			matrix.LockPeriod(new DateOnlyPeriod(day, day));
		}

	}
}
