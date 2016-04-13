using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ScheduleOptimizationTeamBlock : IScheduleOptimization
	{
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly OptimizationResult _optimizationResult;
		private readonly ITeamBlockDayOffOptimizerService _teamBlockDayOffOptimizerService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;

		public ScheduleOptimizationTeamBlock(
			IFillSchedulerStateHolder fillSchedulerStateHolder, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDictionaryPersister persister, 
			IPlanningPeriodRepository planningPeriodRepository,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter, 
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			IMatrixListFactory matrixListFactory,
			DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
			IOptimizerHelperHelper optimizerHelperHelper, 
			ResourceCalculationContextFactory resourceCalculationContextFactory,
			OptimizationResult optimizationResult,
			ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper)
		{
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_matrixListFactory = matrixListFactory;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_optimizationResult = optimizationResult;
			_teamBlockDayOffOptimizerService = teamBlockDayOffOptimizerService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var period = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return _optimizationResult.Create(period);
		}

		[UnitOfWork]
		[LogTime]
		protected virtual DateOnlyPeriod SetupAndOptimize(Guid planningPeriodId)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			var dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;

			_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, period);

			var schedules = schedulerStateHolder.Schedules.SchedulesForPeriod(period, schedulerStateHolder.AllPermittedPersons.FixedStaffPeople(period)).ToList();
			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(schedules);

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForDayOffOptimization, optimizationPreferences, period);

			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences); 
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);

			_groupPersonBuilderWrapper.Reset();
			_groupPersonBuilderWrapper.SetSingleAgentTeam();

			var teamInfoFactory = new TeamInfoFactory(_groupPersonBuilderWrapper);
			
			using (_resourceCalculationContextFactory.Create())
			{
				_teamBlockDayOffOptimizerService.OptimizeDaysOff(
					matrixListForDayOffOptimization, period,
					schedulerStateHolder.AllPermittedPersons, //schedulerStateHolder.SchedulingResultState.PersonsInOrganization.ToList()???
					optimizationPreferences,					
					schedulingOptions,
					resourceCalcDelayer, 
					dayOffOptimizationPreferenceProvider,
					teamInfoFactory,
					new NoSchedulingProgress());

				
				_weeklyRestSolverExecuter.Resolve(
					optimizationPreferences, 
					period, 
					schedules,
					schedulerStateHolder.SchedulingResultState.PersonsInOrganization.ToList(), 
					dayOffOptimizationPreferenceProvider);
			}

			planningPeriod.Scheduled();

			return period;
		}
	}
}