using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class IntradayOptimizerContainerConsiderLargeGroups : IIntradayOptimizerContainer
	{
		private readonly ICurrentIntradayOptimizationCallback _currentIntradayOptimizationCallback;
		private readonly AgentsToSkillSets _agentsToSkillSets;
		private readonly IIntradayOptimizerLimiter _intradayOptimizerLimiter;

		public IntradayOptimizerContainerConsiderLargeGroups(ICurrentIntradayOptimizationCallback currentIntradayOptimizationCallback,
																										AgentsToSkillSets agentsToSkillSets,
																										IIntradayOptimizerLimiter intradayOptimizerLimiter)
		{
			_currentIntradayOptimizationCallback = currentIntradayOptimizationCallback;
			_agentsToSkillSets = agentsToSkillSets;
			_intradayOptimizerLimiter = intradayOptimizerLimiter;
		}

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers)
		{
			var callback = _currentIntradayOptimizationCallback.Current();
			foreach (var agents in _agentsToSkillSets.ToSkillGroups())
			{
				var optimizersToLoop = optimizers.Where(x => agents.Contains(x.ContainerOwner)).ToList();
				var datesToSkip = new IntradayDatesToSkip(_intradayOptimizerLimiter, agents.Count());
				while (optimizersToLoop.Any())
				{
					var optimizer = optimizersToLoop.GetRandom();
					var result = optimizer.Execute(datesToSkip.SkipDates);
					if (result.HasValue)
					{
						datesToSkip.DayWasOptimized(result.Value);
					}
					else
					{
						optimizersToLoop.Remove(optimizer);
					}
					if (callback != null)
					{
						callback.Optimizing(new IntradayOptimizationCallbackInfo(optimizer.ContainerOwner, result.HasValue, optimizersToLoop.Count));
						if (callback.IsCancelled())
							return;
					}
				}
			}
		}
	}
}