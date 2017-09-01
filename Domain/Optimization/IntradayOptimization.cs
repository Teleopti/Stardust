using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Merge into base class", Toggles.ResourcePlanner_SpeedUpShiftsWithinDay_45694)]
	public class IntradayOptimization45694 : IntradayOptimization
	{
		private readonly IntradayOptimizationContext _intradayOptimizationContext;

		public IntradayOptimization45694(IntradayOptimizationContext intradayOptimizationContext,
			TeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService, Func<ISchedulerStateHolder> schedulerStateHolder, IOptimizationPreferencesProvider optimizationPreferencesProvider, MatrixListFactory matrixListFactory, IResourceCalculation resourceCalculation, IUserTimeZone userTimeZone, IScheduleDayChangeCallback scheduleDayChangeCallback, TeamInfoFactoryFactory teamInfoFactoryFactory, ITeamBlockInfoFactory teamBlockInfoFactory, WeeklyRestSolverExecuter weeklyRestSolverExecuter, CascadingResourceCalculationContextFactory resourceCalculationContext) : base(teamBlockIntradayOptimizationService, schedulerStateHolder, optimizationPreferencesProvider, matrixListFactory, resourceCalculation, userTimeZone, scheduleDayChangeCallback, teamInfoFactoryFactory, teamBlockInfoFactory, weeklyRestSolverExecuter, resourceCalculationContext)
		{
			_intradayOptimizationContext = intradayOptimizationContext;
		}

		protected override IDisposable CreateContext(ISchedulerStateHolder stateHolder, DateOnlyPeriod period)
		{
			return _intradayOptimizationContext.Create(period);
		}
	}

	public class IntradayOptimization : IIntradayOptimization
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly WeeklyRestSolverExecuter _weeklyRestSolverExecuter;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpShiftsWithinDay_45694)]
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContext;
		private readonly TeamBlockIntradayOptimizationService _teamBlockIntradayOptimizationService;

		public IntradayOptimization(TeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			MatrixListFactory matrixListFactory,
			IResourceCalculation resourceCalculation, 
			IUserTimeZone userTimeZone,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			TeamInfoFactoryFactory teamInfoFactoryFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			WeeklyRestSolverExecuter weeklyRestSolverExecuter,
			CascadingResourceCalculationContextFactory resourceCalculationContext)
		{
			_teamBlockIntradayOptimizationService = teamBlockIntradayOptimizationService;
			_schedulerStateHolder = schedulerStateHolder;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_matrixListFactory = matrixListFactory;
			_resourceCalculation = resourceCalculation;
			_userTimeZone = userTimeZone;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_weeklyRestSolverExecuter = weeklyRestSolverExecuter;
			_resourceCalculationContext = resourceCalculationContext;
		}

		public void Execute(DateOnlyPeriod period, IEnumerable<IPerson> agents, bool runResolveWeeklyRestRule)
		{
			var stateHolder = _schedulerStateHolder();
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, 1, stateHolder.ConsiderShortBreaks, stateHolder.SchedulingResultState, _userTimeZone);
			var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag));
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(stateHolder.Schedules, stateHolder.SchedulingResultState.PersonsInOrganization, period);
			var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().AllPermittedPersons,_schedulerStateHolder().Schedules, optimizationPreferences.Extra.TeamGroupPage);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory);

			using (CreateContext(stateHolder, period))
			{
				_resourceCalculation.ResourceCalculate(period, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));
				_teamBlockIntradayOptimizationService.Optimize(allMatrixes,
					period,
					agents,
					optimizationPreferences,
					rollbackService,
					resourceCalculateDelayer,
					stateHolder.SchedulingResultState.SkillDays,
					stateHolder.Schedules,
					stateHolder.SchedulingResultState.PersonsInOrganization,
					NewBusinessRuleCollection.AllForScheduling(stateHolder.SchedulingResultState),
					teamBlockGenerator);

				if (runResolveWeeklyRestRule)
				{
					_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, agents, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
				}
			}
		}

		[RemoveMeWithToggle("no need to have seperate method for this", Toggles.ResourcePlanner_SpeedUpShiftsWithinDay_45694)]
		protected virtual IDisposable CreateContext(ISchedulerStateHolder stateHolder, DateOnlyPeriod period)
		{
			return _resourceCalculationContext.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills, true, period);
		}
	}
}