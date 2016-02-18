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
		public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public IntradayOptimizerContainerConsiderLargeGroups(IIntradayOptimizerLimiter intradayOptimizerLimiter)
		{
			_intradayOptimizerLimiter = intradayOptimizerLimiter;
		}

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers, DateOnlyPeriod period)
		{
			if (_intradayOptimizerLimiter.SizeOfGroupLimit < new Percent(0.5))
				return;



			var shuffledOptimizers = optimizers.GetRandom(optimizers.Count(), true);

			var activeOptimizers = shuffledOptimizers.ToList();
			while (activeOptimizers.Count > 0)
			{
				var batchShuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

				foreach (var optimizer in batchShuffledOptimizers)
				{
					var result = optimizer.Execute();
					if (!result)
					{
						activeOptimizers.Remove(optimizer);
					}
				}
			}
		}
	}
}