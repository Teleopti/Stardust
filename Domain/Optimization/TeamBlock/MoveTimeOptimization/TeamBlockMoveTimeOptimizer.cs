﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public interface ITeamBlockMoveTimeOptimizer
	{
		bool OptimizeMatrix(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleMatrixPro matrix);
	}

	public class TeamBlockMoveTimeOptimizer : ITeamBlockMoveTimeOptimizer
	{
		private readonly ISchedulingOptionsCreator  _schedulingOptionsCreator;
		private readonly ITeamBlockMoveTimeDescisionMaker _decisionMaker;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ITeamBlockScheduler  _teamBlockScheduler ;
		private readonly ITeamBlockGenerator _teamBlockGenerator;
		private readonly ILockUnSelectedInTeamBlock _lockUnSelectedInTeamBlock;
		private ITeamBlockRestrictionOverLimitValidator  _teamBlockRestrictionOverLimitValidator;

		public TeamBlockMoveTimeOptimizer(ISchedulingOptionsCreator schedulingOptionsCreator,ITeamBlockMoveTimeDescisionMaker decisionMaker, IDeleteAndResourceCalculateService deleteAndResourceCalculateService, IResourceOptimizationHelper resourceOptimizationHelper,  ITeamBlockScheduler teamBlockScheduler, ITeamBlockGenerator teamBlockGenerator, ILockUnSelectedInTeamBlock lockUnSelectedInTeamBlock, ITeamBlockRestrictionOverLimitValidator teamBlockRestrictionOverLimitValidator)
		{
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_decisionMaker = decisionMaker;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_teamBlockScheduler = teamBlockScheduler;
			_teamBlockGenerator = teamBlockGenerator;
			_lockUnSelectedInTeamBlock = lockUnSelectedInTeamBlock;
			_teamBlockRestrictionOverLimitValidator = teamBlockRestrictionOverLimitValidator;
		}

		public bool OptimizeMatrix(IOptimizationPreferences optimizerPreferences, IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, IPeriodValueCalculator periodValueCalculator, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleMatrixPro matrix)
		{

			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizerPreferences);
			//schedulingOptions.UseCustomTargetTime = _workShiftOriginalStateContainer.OriginalWorkTime();

			double oldPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			IList<DateOnly> daysToBeMoved = _decisionMaker.Execute(matrix ,optimizerPreferences );
			if (daysToBeMoved.Count == 0)
				return false;
			rollbackService.ClearModificationCollection();
			IScheduleDayPro firstDay = matrix.GetScheduleDayByKey(daysToBeMoved[0]);
			DateOnly firstDayDate = daysToBeMoved[0];
			IScheduleDay firstScheduleDay = firstDay.DaySchedulePart();
			var originalFirstScheduleDay = (IScheduleDay)firstScheduleDay.Clone();
			TimeSpan firstDayContractTime = firstDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();

			IScheduleDayPro secondDay = matrix .GetScheduleDayByKey(daysToBeMoved[1]);
			DateOnly secondDayDate = daysToBeMoved[1];
			IScheduleDay secondScheduleDay = secondDay.DaySchedulePart();
			var originalSecondScheduleDay = (IScheduleDay)secondScheduleDay.Clone();
			TimeSpan secondDayContractTime = secondDay.DaySchedulePart().ProjectionService().CreateProjection().ContractTime();

			if (firstDayDate == secondDayDate)
				return false;

			if (firstDayContractTime > secondDayContractTime)
			{
				lockDay(matrix, secondDayDate);
				return true;
			}
				
			//delete schedule on the two days
			IList<IScheduleDay> listToDelete = new List<IScheduleDay> { firstDay.DaySchedulePart(), secondDay.DaySchedulePart() };
			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(listToDelete, rollbackService, schedulingOptions.ConsiderShortBreaks);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true,
																							schedulingOptions.ConsiderShortBreaks);
			var oldHintOptionValue = schedulingOptions.WorkShiftLengthHintOption;
			schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;
			if (!scheduleTeamBlock(matrixList, firstDayDate, matrix, schedulingOptions, rollbackService, resourceCalculateDelayer,schedulingResultStateHolder, originalFirstScheduleDay, TODO))
				return false;
			schedulingOptions.WorkShiftLengthHintOption = oldHintOptionValue;
			if (!scheduleTeamBlock(matrixList, secondDayDate, matrix, schedulingOptions, rollbackService, resourceCalculateDelayer,schedulingResultStateHolder, originalSecondScheduleDay, TODO))
				return false;

		
			double newPeriodValue = periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization);
			bool isPeriodBetter = newPeriodValue < oldPeriodValue;
			if (!isPeriodBetter)
			{
				rollbackService.Rollback();
				lockDay(matrix, firstDayDate );
				lockDay(matrix, secondDayDate);
				return true;
			}

			return true;
		}

		private void safeCalculateDate(DateOnly dayDate, IScheduleDay originalScheduleDay, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			resourceCalculateDelayer.CalculateIfNeeded(dayDate, originalScheduleDay.ProjectionService().CreateProjection().Period());
		}

		private bool scheduleTeamBlock(IList<IScheduleMatrixPro> matrixList, DateOnly dayDate, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService rollbackService, IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleDay originalScheduleDay, IOptimizationPreferences optimizationPreferences)
		{
			var dayTeamBlock = _teamBlockGenerator.Generate(matrixList, new DateOnlyPeriod(dayDate, dayDate),
				new List<IPerson> { matrix.Person }, schedulingOptions).First();
			_lockUnSelectedInTeamBlock.Lock(dayTeamBlock, new List<IPerson> { matrix.Person }, new DateOnlyPeriod(dayDate, dayDate));
			if (_teamBlockScheduler.ScheduleTeamBlockDay(dayTeamBlock, dayDate, schedulingOptions, rollbackService, resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective()))
			{
				if(_teamBlockRestrictionOverLimitValidator.Validate(dayTeamBlock , optimizationPreferences ))
				{
					return true;
				}
			}
			rollbackService.Rollback();
			safeCalculateDate(dayDate, originalScheduleDay, resourceCalculateDelayer);
			return false;
		}

		
		private void lockDay(IScheduleMatrixPro matrix, DateOnly day)
		{
			matrix.LockPeriod(new DateOnlyPeriod(day, day));
		}

	}
}
