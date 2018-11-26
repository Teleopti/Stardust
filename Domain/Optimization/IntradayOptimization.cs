using System;
using System.Collections.Generic;
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
	public class IntradayOptimization
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
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContext;
		private readonly TeamBlockIntradayOptimizationService _teamBlockIntradayOptimizationService;
		private readonly BlockPreferencesMapper _blockPreferencesMapper;
		private readonly PlanningGroupGlobalSettingSetter _planningGroupGlobalSettingSetter;

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
			CascadingResourceCalculationContextFactory resourceCalculationContext, 
			BlockPreferencesMapper blockPreferencesMapper, PlanningGroupGlobalSettingSetter planningGroupGlobalSettingSetter)
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
			_blockPreferencesMapper = blockPreferencesMapper;
			_planningGroupGlobalSettingSetter = planningGroupGlobalSettingSetter;
		}

		public void Execute(DateOnlyPeriod period, IEnumerable<IPerson> agents, bool runResolveWeeklyRestRule, IBlockPreferenceProvider blockPreferenceProvider,
			AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			var stateHolder = _schedulerStateHolder();
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			_planningGroupGlobalSettingSetter.Execute(allSettingsForPlanningGroup, optimizationPreferences);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, stateHolder.ConsiderShortBreaks, stateHolder.SchedulingResultState, _userTimeZone);
			var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag));
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(stateHolder.Schedules, stateHolder.SchedulingResultState.LoadedAgents, period);
			var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().ChoosenAgents,_schedulerStateHolder().Schedules, optimizationPreferences.Extra.TeamGroupPage);
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory, _blockPreferencesMapper);

			using ( _resourceCalculationContext.Create(stateHolder.SchedulingResultState, true, period))
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
					stateHolder.SchedulingResultState.LoadedAgents,
					NewBusinessRuleCollection.MinimumAndPersonAccount(stateHolder.SchedulingResultState, stateHolder.SchedulingResultState.AllPersonAccounts),
					teamBlockGenerator,
					blockPreferenceProvider);

				if (runResolveWeeklyRestRule)
				{
					_weeklyRestSolverExecuter.Resolve(optimizationPreferences, period, agents, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
				}
			}
		}
	}
}