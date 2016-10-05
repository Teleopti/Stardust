﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public class WeeklyRestSolverExecuter
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceOptimization _resourceOptimizationHelper;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public WeeklyRestSolverExecuter(Func<ISchedulerStateHolder> schedulerStateHolder, 
			IResourceOptimization resourceOptimizationHelper, 
			IMatrixListFactory matrixListFactory, 
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void Resolve(IOptimizationPreferences optimizationPreferences, DateOnlyPeriod period, IEnumerable<IScheduleDay> scheduleDays, IList<IPerson> people, 
							IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState,
				_scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulerStateHolder().SchedulingResultState);
			var matrixes = _matrixListFactory.CreateMatrixListForSelection(scheduleDays);

			_weeklyRestSolverCommand.Execute(schedulingOptions,
				optimizationPreferences, people, rollbackService, resourceCalcDelayer, period, matrixes, new NoSchedulingProgress(), dayOffOptimizationPreferenceProvider);
		}
	}
}