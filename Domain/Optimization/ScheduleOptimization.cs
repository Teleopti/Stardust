using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ScheduleOptimization
	{
		private readonly SetupStateHolderForWebScheduling _setupStateHolderForWebScheduling;
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly OptimizationPreferencesFactory _optimizationPreferencesFactory;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;

		public ScheduleOptimization(SetupStateHolderForWebScheduling setupStateHolderForWebScheduling,
			IFixedStaffLoader fixedStaffLoader, Func<ISchedulerStateHolder> schedulerStateHolder,
			IClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand,
			IScheduleDictionaryPersister persister, IPlanningPeriodRepository planningPeriodRepository,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter, OptimizationPreferencesFactory optimizationPreferencesFactory,
			IMatrixListFactory matrixListFactory, IScheduleDayEquator scheduleDayEquator,
			DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
			IOptimizerHelperHelper optimizerHelperHelper)
		{
			_setupStateHolderForWebScheduling = setupStateHolderForWebScheduling;
			_fixedStaffLoader = fixedStaffLoader;
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
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var planningPeriod = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return CreateResult(planningPeriod);
		}

		[LogTime]
		protected virtual OptimizationResultModel CreateResult(IPlanningPeriod planningPeriod)
		{
			var result = new OptimizationResultModel();
			result.Map(_schedulerStateHolder().SchedulingResultState.SkillDays, planningPeriod.Range);
			return result;
		}

		[UnitOfWork]
		[LogTime]
		protected virtual IPlanningPeriod SetupAndOptimize(Guid planningPeriodId)
		{
			var optimizationPreferences = _optimizationPreferencesFactory.Create();
			var dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var people = _fixedStaffLoader.Load(period);
			_setupStateHolderForWebScheduling.Setup(period, people);
			var allSchedules = extractAllSchedules(_schedulerStateHolder().SchedulingResultState, people, period);

			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(allSchedules);
			var matrixOriginalStateContainerListForDayOffOptimization =
				matrixListForDayOffOptimization.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForDayOffOptimization, optimizationPreferences, period);

			_classicDaysOffOptimizationCommand.Execute(matrixOriginalStateContainerListForDayOffOptimization, period, optimizationPreferences, _schedulerStateHolder(),
				new NoBackgroundWorker(), dayOffOptimizationPreferenceProvider);

			_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, allSchedules, people.AllPeople, dayOffOptimizationPreferenceProvider);

			//should maybe happen _after_ all schedules are persisted?
			planningPeriod.Scheduled();

			return planningPeriod;
		}
		
		private static IList<IScheduleDay> extractAllSchedules(ISchedulingResultStateHolder stateHolder, PeopleSelection people, DateOnlyPeriod period)
		{
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules.Where(schedule => people.FixedStaffPeople.Contains(schedule.Key)))
			{
				allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
			}
			return allSchedules;
		}
	}
}