using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
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
		private readonly TeamBlockIntradayOptimizationService _teamBlockIntradayOptimizationService;

		public IntradayOptimization(TeamBlockIntradayOptimizationService teamBlockIntradayOptimizationService,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			MatrixListFactory matrixListFactory,
			IResourceCalculation resourceCalculation, 
			IUserTimeZone userTimeZone,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			TeamInfoFactoryFactory teamInfoFactoryFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory)
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
		}

		public void Execute(DateOnlyPeriod period, IEnumerable<IPerson> agents, bool runResolveWeeklyRestRule)
		{
			//TODO: param runResolveWeeklyRestRule?
			var stateHolder = _schedulerStateHolder();
			var optimizationPreferences = _optimizationPreferencesProvider.Fetch();
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, 1, stateHolder.ConsiderShortBreaks, stateHolder.SchedulingResultState, _userTimeZone);
			//TODO: fel tag? fr�n optpref ist�llet?
			var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, _scheduleDayChangeCallback, new ScheduleTagSetter(optimizationPreferences.General.ScheduleTag));
			var allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(stateHolder.Schedules, stateHolder.SchedulingResultState.PersonsInOrganization, period);
			//TODO: fel! not hard coded hierarchy schedulingOptions.GroupOnGroupPageForTeamBlockPer, beh�ver fixas n�r �ven "teamblocksp�ret" g�r in hit (just nu bara classic)
			var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().AllPermittedPersons,_schedulerStateHolder().Schedules, new GroupPageLight("_", GroupPageType.SingleAgent));
			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory);

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
		}
	}
}