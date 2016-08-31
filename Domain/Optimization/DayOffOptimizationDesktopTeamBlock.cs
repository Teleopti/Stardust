using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationDesktopTeamBlock : IDayOffOptimizationDesktop
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly ITeamBlockDayOffOptimizerService _teamBlockDayOffOptimizerService;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceOptimizationHelperExtended _resouceOptimizationHelperExtended;
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;

		public DayOffOptimizationDesktopTeamBlock(IResourceOptimizationHelper resourceOptimizationHelper,
								ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService,
								Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
								IMatrixListFactory matrixListFactory,
								IOptimizerHelperHelper optimizerHelperHelper,
								Func<ISchedulerStateHolder> schedulerStateHolder,
								IResourceOptimizationHelperExtended resouceOptimizationHelperExtended,
								IResourceCalculationContextFactory resourceCalculationContextFactory,
								TeamInfoFactoryFactory teamInfoFactoryFactory)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_teamBlockDayOffOptimizerService = teamBlockDayOffOptimizerService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_matrixListFactory = matrixListFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_schedulerStateHolder = schedulerStateHolder;
			_resouceOptimizationHelperExtended = resouceOptimizationHelperExtended;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
		}

		public void Execute(DateOnlyPeriod selectedPeriod, 
			IEnumerable<IScheduleDay> selectedDays, 
			ISchedulingProgress backgroundWorker,
			IOptimizationPreferences optimizationPreferences, 
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, 
			GroupPageLight groupPageLight,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var stateHolder = _schedulerStateHolder();

			using (_resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills))
			{
				ResourceCalculationContext.Fetch().PrimarySkillMode = true;

				var matrixList = _matrixListFactory.CreateMatrixListForSelection(selectedDays);

				_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixList, optimizationPreferences, selectedPeriod);

				_resouceOptimizationHelperExtended.ResourceCalculateAllDays(backgroundWorker, false);
				var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences);
				var selectedPersons = matrixList.Select(x => x.Person).Distinct().ToList();

				var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder());
				var teamInfoFactory = _teamInfoFactoryFactory.Create(groupPageLight);

				_teamBlockDayOffOptimizerService.OptimizeDaysOff(matrixList,
					selectedPeriod,
					selectedPersons,
					optimizationPreferences,
					schedulingOptions,
					resourceCalculateDelayer,
					dayOffOptimizationPreferenceProvider,
					teamInfoFactory,
					backgroundWorker);
			}
		}
	}
}