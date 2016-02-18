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
			var numberOfAgents = optimizers.Count(); //this is wrong!
			var optimizedAgents = 0;

			var shuffledOptimizers = optimizers.GetRandom(optimizers.Count(), true);

			var activeOptimizers = shuffledOptimizers.ToList();
			while (activeOptimizers.Count > 0)
			{
				var batchShuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

				foreach (var optimizer in batchShuffledOptimizers)
				{
					if (optimizedAgents/numberOfAgents >= _intradayOptimizerLimiter.MinPercentOfGroupLimit.ValueAsPercent())
					{
						return;
					}

					var result = optimizer.Execute();
					if (!result)
					{
						optimizedAgents++;
						activeOptimizers.Remove(optimizer);
					}
				}
			}
		}
	}
}