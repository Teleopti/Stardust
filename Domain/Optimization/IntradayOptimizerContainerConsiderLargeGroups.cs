using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizerContainerConsiderLargeGroups : IIntradayOptimizerContainer
	{
		private readonly AgentsToSkillGroups _agentsToSkillGroups;
		private readonly IIntradayOptimizerLimiter _intradayOptimizerLimiter;

		public IntradayOptimizerContainerConsiderLargeGroups(AgentsToSkillGroups agentsToSkillGroups,
																										IIntradayOptimizerLimiter intradayOptimizerLimiter)
		{
			_agentsToSkillGroups = agentsToSkillGroups;
			_intradayOptimizerLimiter = intradayOptimizerLimiter;
		}

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers)
		{
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
				}
			}
		}
	}
}