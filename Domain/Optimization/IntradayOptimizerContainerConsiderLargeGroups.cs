using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizerContainerConsiderLargeGroups : IIntradayOptimizerContainer
	{
		private readonly ICurrentIntradayOptimizationCallback _currentIntradayOptimizationCallback;
		private readonly AgentsToSkillGroups _agentsToSkillGroups;
		private readonly IIntradayOptimizerLimiter _intradayOptimizerLimiter;

		public IntradayOptimizerContainerConsiderLargeGroups(ICurrentIntradayOptimizationCallback currentIntradayOptimizationCallback,
																										AgentsToSkillGroups agentsToSkillGroups,
																										IIntradayOptimizerLimiter intradayOptimizerLimiter)
		{
			_currentIntradayOptimizationCallback = currentIntradayOptimizationCallback;
			_agentsToSkillGroups = agentsToSkillGroups;
			_intradayOptimizerLimiter = intradayOptimizerLimiter;
		}

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers)
		{
			var callback = _currentIntradayOptimizationCallback.Current();
			foreach (var agents in _agentsToSkillGroups.ToSkillGroups())
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
						callback.Optimizing(new IntradayOptimizationCallbackInfo(optimizer.ContainerOwner, result.HasValue, optimizersToLoop.Count)); //these numbers are just strange - tried to keep as before, but needs to be looked at
						if (callback.IsCancelled())
							return;
					}
				}
			}
		}
	}
}