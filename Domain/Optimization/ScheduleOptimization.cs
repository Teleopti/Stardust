using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
	public class ScheduleOptimization : IScheduleOptimization
	{
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ClassicDaysOffOptimizationCommand _classicDaysOffOptimizationCommand;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly OptimizationResult _optimizationResult;
		private readonly IResourceOptimizationHelperExtended _resourceOptimizationHelperExtended;
		private readonly IPersonRepository _personRepository;

		public ScheduleOptimization(IFillSchedulerStateHolder fillSchedulerStateHolder, Func<ISchedulerStateHolder> schedulerStateHolder,
			ClassicDaysOffOptimizationCommand classicDaysOffOptimizationCommand,
			IScheduleDictionaryPersister persister, IPlanningPeriodRepository planningPeriodRepository,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter, IOptimizationPreferencesProvider optimizationPreferencesProvider,
			IMatrixListFactory matrixListFactory, IScheduleDayEquator scheduleDayEquator,
			DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory,
			IOptimizerHelperHelper optimizerHelperHelper, CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			OptimizationResult optimizationResult, IResourceOptimizationHelperExtended resourceOptimizationHelperExtended,
			IPersonRepository personRepository)
		{
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_classicDaysOffOptimizationCommand = classicDaysOffOptimizationCommand;
			_persister = persister;
			_planningPeriodRepository = planningPeriodRepository;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_matrixListFactory = matrixListFactory;
			_scheduleDayEquator = scheduleDayEquator;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
			_optimizerHelperHelper = optimizerHelperHelper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_optimizationResult = optimizationResult;
			_resourceOptimizationHelperExtended = resourceOptimizationHelperExtended;
			_personRepository = personRepository;
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var period = SetupAndOptimize(planningPeriodId);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return _optimizationResult.Create(period);
		}

		[UnitOfWork]
		[TestLog]
		protected virtual DateOnlyPeriod SetupAndOptimize(Guid planningPeriodId)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider;
			
			var period = planningPeriod.Range;
			var agentGroup = planningPeriod.AgentGroup;
			if (agentGroup == null)
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, null, period);
			}
			else
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create(agentGroup);
				var people = _personRepository.FindPeopleInAgentGroup(agentGroup, period);
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, people.Select(x => x.Id.Value), null, null, period);
			}
			
			var schedules = schedulerStateHolder.Schedules.SchedulesForPeriod(period, schedulerStateHolder.AllPermittedPersons.FixedStaffPeople(period)).ToArray();

			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, schedules);
			var matrixOriginalStateContainerListForDayOffOptimization =
				matrixListForDayOffOptimization.Select(matrixPro => new ScheduleMatrixOriginalStateContainer(matrixPro, _scheduleDayEquator))
					.Cast<IScheduleMatrixOriginalStateContainer>().ToList();

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForDayOffOptimization, optimizationPreferences, period);

#pragma warning disable 618
			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, true))
#pragma warning restore 618
			{
				_resourceOptimizationHelperExtended.ResourceCalculateAllDays(new NoSchedulingProgress(), false);
				_classicDaysOffOptimizationCommand.Execute(matrixOriginalStateContainerListForDayOffOptimization, period,
					optimizationPreferences, new NoSchedulingProgress(), dayOffOptimizationPreferenceProvider);

				_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, schedules,
					schedulerStateHolder.SchedulingResultState.PersonsInOrganization.ToList(), dayOffOptimizationPreferenceProvider);
			}

			//should maybe happen _after_ all schedules are persisted?
			planningPeriod.Scheduled();

			return period;
		}
	}
}