using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizerContainerConsiderLargeGroups : IIntradayOptimizerContainer
	{
		private readonly AgentsToSkillGroups _agentsToSkillGroups;
		private readonly IIntradayOptimizerLimiter _intradayOptimizerLimiter;

		//försök ta bort event senare...
#pragma warning disable 0067
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
#pragma warning restore 0067

		public IntradayOptimizerContainerConsiderLargeGroups(AgentsToSkillGroups agentsToSkillGroups,
																										IIntradayOptimizerLimiter intradayOptimizerLimiter)
		{
			_agentsToSkillGroups = agentsToSkillGroups;
			_intradayOptimizerLimiter = intradayOptimizerLimiter;
		}

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers)
		{
			var agentSkillGroups = _agentsToSkillGroups.ToSkillGroups();
			foreach (var agents in agentSkillGroups)
			{
				var optimizersToLoop = optimizers.Where(x => agents.Contains(x.ContainerOwner)).ToList();
				var numberOfAgents = agents.Count();
				var optimizedPerDay = new Dictionary<DateOnly, optimizeCounter>();
				var skipDates = new List<DateOnly>();

				while (optimizersToLoop.Any())
				{
					var optimizer = optimizersToLoop.GetRandom();
					var result = optimizer.Execute(skipDates);
					if (result.HasValue)
					{
						optimizeCounter optimizeCounter;
						if (!optimizedPerDay.TryGetValue(result.Value, out optimizeCounter))
						{
							optimizeCounter = new optimizeCounter(_intradayOptimizerLimiter);
							optimizedPerDay[result.Value] = optimizeCounter;
						}

						optimizeCounter.Increase();
						if (optimizeCounter.HasReachedLimit(numberOfAgents))
						{
							skipDates.Add(result.Value);
						}
					}
					else
					{
						optimizersToLoop.Remove(optimizer);
					}
				}
			}
		}

		private class optimizeCounter
		{
			private readonly IIntradayOptimizerLimiter _intradayOptimizerLimiter;
			private int numberOfOptimizations;

			public optimizeCounter(IIntradayOptimizerLimiter intradayOptimizerLimiter)
			{
				_intradayOptimizerLimiter = intradayOptimizerLimiter;
			}

			public void Increase()
			{
				numberOfOptimizations++;
			}

			public bool HasReachedLimit(int numberOfAgentsInGroup)
			{
				return _intradayOptimizerLimiter.CanJumpOutEarly(numberOfAgentsInGroup, numberOfOptimizations);
			}
		}
	}
}