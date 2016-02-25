using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ScheduleOptimization
	{
		private readonly WebSchedulingSetup _webSchedulingSetup;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;

		public ScheduleOptimization(WebSchedulingSetup webSchedulingSetup, Func<ISchedulerStateHolder> schedulerStateHolder,
			ClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand,
			IScheduleDictionaryPersister persister, IPlanningPeriodRepository planningPeriodRepository,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter, OptimizationPreferencesFactory optimizationPreferencesFactory,
			IMatrixListFactory matrixListFactory, IScheduleDayEquator scheduleDayEquator,
			DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
			IOptimizerHelperHelper optimizerHelperHelper, ResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_webSchedulingSetup = webSchedulingSetup;
			_schedulerStateHolder = schedulerStateHolder;
			_classicDaysOffOptimizationCommand = classicDaysOffOptimizationCommand;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_optimizationPreferencesFactory = optimizationPreferencesFactory;
			_matrixListFactory = matrixListFactory;
			_scheduleDayEquator = scheduleDayEquator;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var period = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return CreateResult(period);
		}

		[LogTime]
		protected virtual OptimizationResultModel CreateResult(DateOnlyPeriod period)
		{
			var result = new OptimizationResultModel();
			result.Map(_schedulerStateHolder().SchedulingResultState.SkillDays, period);
			return result;
		}

		[UnitOfWork]
		[LogTime]
		protected virtual DateOnlyPeriod SetupAndOptimize(Guid planningPeriodId)
		{
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var webScheduleState = _webSchedulingSetup.Setup(period);

			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(webScheduleState.AllSchedules);
			var matrixOriginalStateContainerListForDayOffOptimization =
				matrixListForDayOffOptimization.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForDayOffOptimization, optimizationPreferences, period);

			using (_resourceCalculationContextFactory.Create())
			{
				_classicDaysOffOptimizationCommand.Execute(matrixOriginalStateContainerListForDayOffOptimization, period,
					optimizationPreferences, _schedulerStateHolder(),
					new NoSchedulingProgress(), dayOffOptimizationPreferenceProvider);

				_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, webScheduleState.AllSchedules,
					webScheduleState.PeopleSelection.AllPeople, dayOffOptimizationPreferenceProvider);
			}

			//should maybe happen _after_ all schedules are persisted?
			planningPeriod.Scheduled();

			return period;
		}
	}
}