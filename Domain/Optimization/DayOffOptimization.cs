using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimization
	{
		private readonly TeamBlockDayOffOptimizer _teamBlockDayOffOptimizer;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public DayOffOptimization(TeamBlockDayOffOptimizer teamBlockDayOffOptimizer,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter,
			IResourceCalculation resourceCalculation,
			Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_teamBlockDayOffOptimizer = teamBlockDayOffOptimizer;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_resourceCalculation = resourceCalculation;
			_schedulerStateHolder = schedulerStateHolder;
		}
		
		public void Execute(IEnumerable<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences, SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IBlockPreferenceProvider blockPreferenceProvider, ITeamInfoFactory teamInfoFactory,
			ISchedulingProgress backgroundWorker, bool runWeeklyRestSolver)
		{
			_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(_schedulerStateHolder().SchedulingResultState, false, false));
			
			_teamBlockDayOffOptimizer.OptimizeDaysOff(matrixList,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulingOptions,
				resourceCalculateDelayer,
				dayOffOptimizationPreferenceProvider,
				blockPreferenceProvider,
				teamInfoFactory,
				backgroundWorker);
			if (runWeeklyRestSolver)
			{
				_weeklyRestSolverExecuter.Resolve(
					optimizationPreferences, 
					selectedPeriod,
					selectedPersons, 
					dayOffOptimizationPreferenceProvider);
			}
		}
	}
}