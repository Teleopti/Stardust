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
			var numberOfAgents = optimizers.Count(); //this is wrong - should be per skillgroup (also maybe not counting duplicate scheduleperiods if important)

			foreach (var date in period.DayCollection().RandomIterator())
			{
				var optimizedAgents = 0;
				foreach (var optimizer in optimizers)
				{
					if (_intradayOptimizerLimiter.CanJumpOutEarly(numberOfAgents, optimizedAgents))
						break;

					//borde kunna ta bort optimizern här om false
					optimizer.IntradayOptimizeOneday.Execute(date);
					optimizedAgents++;
				}
			}
		}
	}
}