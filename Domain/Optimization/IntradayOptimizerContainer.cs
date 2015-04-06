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
        private readonly IList<IIntradayOptimizer2> _optimizers;
	    private readonly IDailyValueByAllSkillsExtractor _dailyValueByAllSkillsExtractor;
	    private bool _cancelMe;
		private ResourceOptimizerProgressEventArgs _progressEvent;

        public IntradayOptimizerContainer(IList<IIntradayOptimizer2> optimizers, IDailyValueByAllSkillsExtractor dailyValueByAllSkillsExtractor)
        {
	        _optimizers = optimizers;
	        _dailyValueByAllSkillsExtractor = dailyValueByAllSkillsExtractor;
        }

	    public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Execute(DateOnlyPeriod period, TargetValueOptions targetValueOptions)
        {
			_progressEvent = null;

            if (_cancelMe)
                return;

			IEnumerable<IIntradayOptimizer2> shuffledOptimizers = _optimizers.GetRandom(_optimizers.Count, true);
	        var log = new List<string>();
			foreach (var batchOptimizers in shuffledOptimizers.Batch(100))
			{
				var valueBefore = _dailyValueByAllSkillsExtractor.ValueForPeriod(period, targetValueOptions);
				executeOptimizersWhileActiveFound(batchOptimizers);
				var valueAfter = _dailyValueByAllSkillsExtractor.ValueForPeriod(period, targetValueOptions);
				var logPost = DateTime.Now.ToLongTimeString() + " " + (valueBefore - valueAfter);
				log.Add(logPost);
			}
	        foreach (var post in log)
	        {
		        Debug.Print(post);
	        }
        }

        public void OnReportProgress(string message)
        {
        	var handler = ReportProgress;
            if (handler != null)
            {
                ResourceOptimizerProgressEventArgs args = new ResourceOptimizerProgressEventArgs(0, 0, message);
                handler(this, args);
                if (args.Cancel)
                    _cancelMe = true;

				if (_progressEvent != null && _progressEvent.UserCancel) return;
				_progressEvent = args;
            }
        }

        /// <summary>
        /// Runs the active optimizers while at least one is active and can do more optimization.
        /// </summary>
        /// <param name="optimizers">All optimizer containers.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.IntradayOptimizerContainer.OnReportProgress(System.String)")]
        private void executeOptimizersWhileActiveFound(IEnumerable<IIntradayOptimizer2> optimizers)
        {
            IList<IIntradayOptimizer2> activeOptimizers =
                new List<IIntradayOptimizer2>(optimizers);

            while (activeOptimizers.Count > 0)
            {
                if (_cancelMe)
                    break;

				if (_progressEvent != null && _progressEvent.UserCancel)
					break;

                IEnumerable<IIntradayOptimizer2> shuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

                int executes = 0;
                foreach (IIntradayOptimizer2 optimizer in shuffledOptimizers)
                {
                    if (_cancelMe)
                        break;

					if (_progressEvent != null && _progressEvent.UserCancel)
						break;

                    executes++;
                    bool result = optimizer.Execute();

                    if (!result)
                    {
                        activeOptimizers.Remove(optimizer);
                    }
                     string who = Resources.OptimizingIntraday + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + optimizer.ContainerOwner.Name.ToString(NameOrderOption.FirstNameLastName);
                    string success;
                    if (!result)
                    {
                        success = " " + Resources.wasNotSuccessful;
                    }
                    else
                    {
                        success = " " + Resources.wasSuccessful;
                    }
                    OnReportProgress(who + success);
                }
            }
        }
    }
}
