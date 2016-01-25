using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class IntradayOptimizerContainer
    {
	    private readonly IDailyValueByAllSkillsExtractor _dailyValueByAllSkillsExtractor;

        public IntradayOptimizerContainer(IDailyValueByAllSkillsExtractor dailyValueByAllSkillsExtractor)
        {
	        _dailyValueByAllSkillsExtractor = dailyValueByAllSkillsExtractor;
        }

	    public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Execute(IList<IIntradayOptimizer2> optimizers, DateOnlyPeriod period, TargetValueOptions targetValueOptions)
        {
			IEnumerable<IIntradayOptimizer2> shuffledOptimizers = optimizers.GetRandom(optimizers.Count, true);
#if DEBUG
	        var log = new List<string>();
#endif
			var cancel = false;
			foreach (var batchOptimizers in shuffledOptimizers.Batch(100))
			{
#if DEBUG
				var valueBefore = _dailyValueByAllSkillsExtractor.ValueForPeriod(period, targetValueOptions);
#endif
				IList<IIntradayOptimizer2> activeOptimizers = new List<IIntradayOptimizer2>(batchOptimizers);
				while (activeOptimizers.Count > 0)
				{
					IEnumerable<IIntradayOptimizer2> batchShuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

					int executes = 0;
					foreach (IIntradayOptimizer2 optimizer in batchShuffledOptimizers)
					{
						if (cancel) return;
						executes++;
						bool result = optimizer.Execute();

						if (!result)
						{
							activeOptimizers.Remove(optimizer);
						}
						string who = Resources.OptimizingIntraday + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + optimizer.ContainerOwner.Name.ToString(NameOrderOption.FirstNameLastName);
						string success = !result ? " " + Resources.wasNotSuccessful : " " + Resources.wasSuccessful;
						var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, who + success, ()=>cancel=true));
						if (cancel || progressResult.ShouldCancel) return;
					}
				}
#if DEBUG
				var valueAfter = _dailyValueByAllSkillsExtractor.ValueForPeriod(period, targetValueOptions);
				var logPost = DateTime.Now.ToLongTimeString() + " " + (valueBefore - valueAfter);
				log.Add(logPost);
#endif
			}
#if DEBUG
	        foreach (var post in log)
	        {
		        Debug.Print(post);
	        }
#endif
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
