using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
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
	    public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Execute(IEnumerable<IIntradayOptimizer2> optimizers, DateOnlyPeriod period)
		{
			var shuffledOptimizers = optimizers.GetRandom(optimizers.Count(), true);

			var cancel = false;
			foreach (var batchOptimizers in shuffledOptimizers.Batch(100))
			{
				var activeOptimizers = batchOptimizers.ToList();
				while (activeOptimizers.Count > 0)
				{
					var batchShuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

					var executes = 0;
					foreach (var optimizer in batchShuffledOptimizers)
					{
						if (cancel) return;
						executes++;
						var result = optimizer.Execute(Enumerable.Empty<DateOnly>());

						if (!result.HasValue)
						{
							activeOptimizers.Remove(optimizer);
						}
						var who = Resources.OptimizingIntraday + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " +
										optimizer.ContainerOwner.Name.ToString(NameOrderOption.FirstNameLastName);
						var success = !result.HasValue ? " " + Resources.wasNotSuccessful : " " + Resources.wasSuccessful;
						var progressResult =
							onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, who + success, () => cancel = true));
						if (cancel || progressResult.ShouldCancel) return;
					}
				}
			}
		}

        private CancelSignal onReportProgress(ResourceOptimizerProgressEventArgs args)
        {
        	var handler = ReportProgress;
            if (handler != null)
            {
                handler(this, args);
                if (args.Cancel) return new CancelSignal{ShouldCancel = true};
            }
			return new CancelSignal();
        }
    }
}
