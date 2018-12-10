using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public class WeeklyRestSolverExecuter
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly WeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IUserTimeZone _userTimeZone;

		public WeeklyRestSolverExecuter(Func<ISchedulerStateHolder> schedulerStateHolder, 
			IResourceCalculation resourceOptimizationHelper,
			MatrixListFactory matrixListFactory, 
			WeeklyRestSolverCommand weeklyRestSolverCommand,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IUserTimeZone userTimeZone)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_userTimeZone = userTimeZone;
		}

		public void Resolve(IOptimizationPreferences optimizationPreferences, DateOnlyPeriod period, IEnumerable<IPerson> people, 
							IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState,
				_scheduleDayChangeCallback, new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, schedulingOptions.ConsiderShortBreaks, _schedulerStateHolder().SchedulingResultState, _userTimeZone);
			var matrixes = _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, people, period);

			_weeklyRestSolverCommand.Execute(schedulingOptions,
				optimizationPreferences, people, rollbackService, resourceCalcDelayer, period, matrixes, new NoSchedulingProgress(), dayOffOptimizationPreferenceProvider);
		}
	}
}