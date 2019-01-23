using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationExecutor
	{
		private readonly IntradayOptimization _intradayOptimization;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ISynchronizeSchedulesAfterIsland _synchronizeSchedulesAfterIsland;
		private readonly IGridlockManager _gridlockManager;
		private readonly IBlockPreferenceProviderForPlanningPeriod _blockPreferenceProviderForPlanningPeriod;
		private readonly DeadLockRetrier _deadLockRetrier;
		private readonly IPlanningGroupSettingsProvider _planningGroupSettingsProvider;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public IntradayOptimizationExecutor(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			FillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeSchedulesAfterIsland synchronizeSchedulesAfterIsland,
			IGridlockManager gridlockManager,
			IBlockPreferenceProviderForPlanningPeriod blockPreferenceProviderForPlanningPeriod,
			DeadLockRetrier deadLockRetrier,
			IPlanningGroupSettingsProvider planningGroupSettingsProvider, 
			IPlanningPeriodRepository planningPeriodRepository)
		{
			_intradayOptimization = intradayOptimization;
			_schedulerStateHolder = schedulerStateHolder;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_synchronizeSchedulesAfterIsland = synchronizeSchedulesAfterIsland;
			_gridlockManager = gridlockManager;
			_blockPreferenceProviderForPlanningPeriod = blockPreferenceProviderForPlanningPeriod;
			_deadLockRetrier = deadLockRetrier;
			_planningGroupSettingsProvider = planningGroupSettingsProvider;
			_planningPeriodRepository = planningPeriodRepository;
		}

		[TestLog]
		public virtual void HandleEvent(IntradayOptimizationWasOrdered @event)
		{
			_deadLockRetrier.RetryOnDeadlock(() =>
			{
				var period = new DateOnlyPeriod(@event.StartDate, @event.EndDate);
				DoOptimization(period, @event.AgentsInIsland, @event.Agents, @event.UserLocks, @event.Skills, @event.RunResolveWeeklyRestRule, @event.PlanningPeriodId);
				_synchronizeSchedulesAfterIsland.Synchronize(_schedulerStateHolder().Schedules, period);
			});
		}
		
		[ReadonlyUnitOfWork]
		protected virtual void DoOptimization(
			DateOnlyPeriod period,
			IEnumerable<Guid> agentsInIsland,
			IEnumerable<Guid> agentsToOptimize,
			IEnumerable<LockInfo> locks,
			IEnumerable<Guid> onlyUseSkills,
			bool runResolveWeeklyRestRule,
			Guid planningPeriodId)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var allSettingsForPlanningGroup = _planningGroupSettingsProvider.Execute(planningPeriodId);
			var blockPreferenceProvider = _blockPreferenceProviderForPlanningPeriod.Fetch(allSettingsForPlanningGroup);
			_fillSchedulerStateHolder.Fill(schedulerStateHolder, agentsInIsland, new LockInfoForStateHolder(_gridlockManager, locks), period, onlyUseSkills);
			var agents = schedulerStateHolder.ChoosenAgents.Filter(agentsToOptimize);
			if (planningPeriodId == Guid.Empty)
			{
				_intradayOptimization.Execute(period, agents, runResolveWeeklyRestRule, blockPreferenceProvider, allSettingsForPlanningGroup);
			}
			else
			{
				var lastJobResult = _planningPeriodRepository.Get(planningPeriodId).GetLastSchedulingJob();
				var agentIdsWithPreferenceHints = lastJobResult == null
					? new HashSet<Guid>()
					: JsonConvert.DeserializeObject<FullSchedulingResultModel>(lastJobResult.Details.Last().Message)
						.BusinessRulesValidationResults.Where(x => x.ValidationErrors.Any(y => y.ResourceType == ValidationResourceType.Preferences))
						.Select(x => x.ResourceId).ToHashSet();
				var agentsToOptimizeWithoutPreferences = agents.Where(agent => agentIdsWithPreferenceHints.Contains(agent.Id.Value)).ToList();
				var agentsToOptimizeWithOriginalPreferenceValues = agents.Except(agentsToOptimizeWithoutPreferences).ToArray();
				using (allSettingsForPlanningGroup.DontUsePreferences())
				{
					_intradayOptimization.Execute(period, agentsToOptimizeWithoutPreferences, runResolveWeeklyRestRule, blockPreferenceProvider, allSettingsForPlanningGroup);
				}
				_intradayOptimization.Execute(period, agentsToOptimizeWithOriginalPreferenceValues, runResolveWeeklyRestRule, blockPreferenceProvider, allSettingsForPlanningGroup);
			}
		}
	}
}