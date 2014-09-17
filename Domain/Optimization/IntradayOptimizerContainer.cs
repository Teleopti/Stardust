using System;
using System.Collections.Generic;
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
    public class IntradayOptimizerContainer : IScheduleOptimizationService
    {
        private readonly IList<IIntradayOptimizer2> _optimizers;
        private bool _cancelMe;
		private ResourceOptimizerProgressEventArgs _progressEvent;

        public IntradayOptimizerContainer(IList<IIntradayOptimizer2> optimizers)
        {
            _optimizers = optimizers;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void Execute()
        {
			_progressEvent = null;

            if (_cancelMe)
                return;

            executeOptimizersWhileActiveFound(_optimizers);
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
