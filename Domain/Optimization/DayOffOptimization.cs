using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
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
		private readonly IUserTimeZone _userTimeZone;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;

		public DayOffOptimization(TeamBlockDayOffOptimizer teamBlockDayOffOptimizer,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter,
			IResourceCalculation resourceCalculation,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IUserTimeZone userTimeZone,
			TeamInfoFactoryFactory teamInfoFactoryFactory)
		{
			_teamBlockDayOffOptimizer = teamBlockDayOffOptimizer;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_resourceCalculation = resourceCalculation;
			_schedulerStateHolder = schedulerStateHolder;
			_userTimeZone = userTimeZone;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
		}
		
		public void Execute(IEnumerable<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences, SchedulingOptions schedulingOptions,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IBlockPreferenceProvider blockPreferenceProvider,
			ISchedulingProgress backgroundWorker, bool runWeeklyRestSolver)
		{
			var stateHolder = _schedulerStateHolder();
			
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ConsiderShortBreaks, stateHolder.SchedulingResultState, _userTimeZone);
			
			_resourceCalculation.ResourceCalculate(selectedPeriod.Inflate(1), new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));

			///////////////////////////////
			ITeamInfoFactory teamInfoFactory;
			//HACK! Verify what is right!!!!
			if (runWeeklyRestSolver) //web - just keep old behavior for now
			{
				teamInfoFactory = _teamInfoFactoryFactory.Create(selectedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer); 
			}
			else //win - just keep old behavior for now
			{
				teamInfoFactory = _teamInfoFactoryFactory.Create(stateHolder.ChoosenAgents, stateHolder.Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			}
			/////////////////////////////////
			
			_teamBlockDayOffOptimizer.OptimizeDaysOff(matrixList,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulingOptions,
				resourceCalcDelayer,
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