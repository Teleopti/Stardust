using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	/// <summary>
    /// Contains the logic of an intraday optimization iteration for one matrix.
    /// - Order the list of IntradayOptimizers
    /// - Manages the list of IntradayOptimizers according to the result of Optimizers
    /// </summary>
    public class IntradayOptimizerContainer : IIntradayOptimizerContainer
    {
		private readonly IntradayOptimizationCallbackContext _intradayOptimizationCallbackContext;

		public IntradayOptimizerContainer(IntradayOptimizationCallbackContext intradayOptimizationCallbackContext)
		{
			_intradayOptimizationCallbackContext = intradayOptimizationCallbackContext;
		}

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers)
		{
			var shuffledOptimizers = optimizers.GetRandom(optimizers.Count(), true);
			var callback = _intradayOptimizationCallbackContext.Current();

			foreach (var batchOptimizers in shuffledOptimizers.Batch(100))
			{
				var activeOptimizers = batchOptimizers.ToList();
				while (activeOptimizers.Count > 0)
				{
					var batchShuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

					var executes = 0;
					foreach (var optimizer in batchShuffledOptimizers)
					{
						executes++;
						var result = optimizer.Execute(Enumerable.Empty<DateOnly>());

						if (!result.HasValue)
						{
							activeOptimizers.Remove(optimizer);
						}
						callback.Optimizing(new IntradayOptimizationCallbackInfo(optimizer.ContainerOwner, result.HasValue, activeOptimizers.Count, executes));
						if (callback.IsCancelled())
							return;
					}
				}
			}
		}
    }
}
