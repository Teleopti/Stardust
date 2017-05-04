using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

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
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly OptimizationResult _optimizationResult;
		private readonly TeamBlockDayOffOptimizer _teamBlockDayOffOptimizer;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPersonRepository _personRepository;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;

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
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory,
			OptimizationResult optimizationResult,
			TeamBlockDayOffOptimizer teamBlockDayOffOptimizer,
			IResourceCalculation resourceOptimizationHelper,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IUserTimeZone userTimeZone,
			IPersonRepository personRepository,
			IResourceCalculation resourceCalculation,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory)
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
			_teamBlockDayOffOptimizer = teamBlockDayOffOptimizer;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_userTimeZone = userTimeZone;
			_personRepository = personRepository;
			_resourceCalculation = resourceCalculation;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
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
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider;
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var agentGroup = planningPeriod.AgentGroup;
			IScheduleDay[] schedules;
			if (agentGroup == null)
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create();
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, null, period);
				schedules = schedulerStateHolder.Schedules.SchedulesForPeriod(period, schedulerStateHolder.AllPermittedPersons.FixedStaffPeople(period)).ToArray();
			}
			else
			{
				dayOffOptimizationPreferenceProvider = _dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create(agentGroup);
				var people = _personRepository.FindPeopleInAgentGroup(agentGroup, period);
				var skills = new HashSet<Guid>(people
					.SelectMany(person => person.PersonPeriods(period))
					.SelectMany(pp => pp.PersonSkillCollection)
					.Select(personSkill => personSkill.Skill)
					.Select(skill => skill.Id.GetValueOrDefault()));
				_fillSchedulerStateHolder.Fill(schedulerStateHolder, null, null, null, period, skills);
				
				schedules = schedulerStateHolder.Schedules.SchedulesForPeriod(period, people.FixedStaffPeople(period)).ToArray();
			}

			var matrixListForDayOffOptimization = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.PersonsInOrganization, period); 

			_optimizerHelperHelper.LockDaysForDayOffOptimization(matrixListForDayOffOptimization, optimizationPreferences, period);

			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizationPreferences); 
			var resourceCalcDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks, schedulerStateHolder.SchedulingResultState, _userTimeZone);

			//copied from other places -what is this!? needed to get green test
			var groupPageType = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type;
			if (groupPageType == GroupPageType.SingleAgent)
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			else
				_groupPersonBuilderForOptimizationFactory.Create(_schedulerStateHolder().AllPermittedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);
			//

			using (_resourceCalculationContextFactory.Create(schedulerStateHolder.Schedules, schedulerStateHolder.SchedulingResultState.Skills, true, period.Inflate(1)))
			{
				_resourceCalculation.ResourceCalculate(period.Inflate(1), new ResourceCalculationData(schedulerStateHolder.SchedulingResultState, false, false));
				_teamBlockDayOffOptimizer.OptimizeDaysOff(
					matrixListForDayOffOptimization, period,
					schedulerStateHolder.SchedulingResultState.PersonsInOrganization.ToList(),
					optimizationPreferences,
					schedulingOptions,
					resourceCalcDelayer, 
					dayOffOptimizationPreferenceProvider,
					new TeamInfoFactory(_groupPersonBuilderWrapper),
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