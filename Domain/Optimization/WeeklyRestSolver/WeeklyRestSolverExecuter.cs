using System;
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
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;

		public WeeklyRestSolverExecuter(Func<ISchedulerStateHolder> schedulerStateHolder, 
			IResourceOptimizationHelper resourceOptimizationHelper, 
			IMatrixListFactory matrixListFactory, 
			IWeeklyRestSolverCommand weeklyRestSolverCommand)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
		}

		public void Resolve(IOptimizationPreferences optimizationPreferences, DateOnlyPeriod period, IList<IScheduleDay> scheduleDays, IList<IPerson> people, 
							IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
			var rollbackService = new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState,
				new DoNothingScheduleDayChangeCallBack(), new ScheduleTagSetter(KeepOriginalScheduleTag.Instance));
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);
			var matrixes = _matrixListFactory.CreateMatrixList(scheduleDays, period);

			_weeklyRestSolverCommand.Execute(schedulingOptions,
				optimizationPreferences, people, rollbackService, resourceCalcDelayer, period, matrixes, new NoBackgroundWorker(), dayOffOptimizationPreferenceProvider);
		}
	}
}