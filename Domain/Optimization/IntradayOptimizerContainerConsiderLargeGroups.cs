using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizerContainerConsiderLargeGroups : IIntradayOptimizerContainer
	{
		private readonly IIntradayOptimizerLimiter _intradayOptimizerLimiter;

		//försök ta bort event senare...
#pragma warning disable 0067
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
#pragma warning restore 0067

		public IntradayOptimizerContainerConsiderLargeGroups(IIntradayOptimizerLimiter intradayOptimizerLimiter)
		{
			_intradayOptimizerLimiter = intradayOptimizerLimiter;
		}

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers, DateOnlyPeriod period)
		{
			var optimizersToLoop = optimizers.ToList();
			var numberOfAgents = optimizersToLoop.Select(x => x.ContainerOwner).Distinct().Count();
			var optimizedPerDay = new Dictionary<DateOnly, int>();

			while (optimizersToLoop.Any())
			{
				var optimizer = optimizersToLoop.GetRandom();
				var result = optimizer.Execute();
				if (result.HasValue)
				{
					//TODO: currently 4 lookups in dic -> reduce if necessary
					if (optimizedPerDay.ContainsKey(result.Value))
					{
						optimizedPerDay[result.Value]++;
					}
					else
					{
						optimizedPerDay[result.Value] = 1;
					}
					if (_intradayOptimizerLimiter.CanJumpOutEarly(numberOfAgents, optimizedPerDay[result.Value]))
					{
						optimizersToLoop.ForEach(x => x.LockDay(result.Value));
					}
				}
				else
				{
					optimizersToLoop.Remove(optimizer);
				}
			}
		}
	}
}