using System;
using System.Collections.Generic;
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
		private readonly MatrixListFactory _matrixListFactory;
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
			MatrixListFactory matrixListFactory, IScheduleDayEquator scheduleDayEquator,
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
			return _optimizationResult.Create(period, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period).ToList());
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
			var planningGroup = planningPeriod.PlanningGroup;
			IEnumerable<IPerson> agents;
			if (planningGroup == null)
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
				
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period);
				agents = schedulerStateHolder.AllPermittedPersons.FixedStaffPeople(period);
			}
			else
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create(planningGroup);
				var people = _personRepository.FindPeopleInPlanningGroup(planningGroup, period);
				var skills = new HashSet<Guid>(people
					.SelectMany(person => person.PersonPeriods(period))
					.SelectMany(pp => pp.PersonSkillCollection)
					.Select(personSkill => personSkill.Skill)
					.Select(skill => skill.Id.GetValueOrDefault()));
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, period, skills);
				agents = people.FixedStaffPeople(period);
			}

			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListForSelection(schedulerStateHolder.Schedules, agents, period);
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

				_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, agents.ToList(), dayOffOptimizationPreferenceProvider);
			}

			//should maybe happen _after_ all schedules are persisted?
			planningPeriod.Scheduled();

			return period;
		}
	}
}