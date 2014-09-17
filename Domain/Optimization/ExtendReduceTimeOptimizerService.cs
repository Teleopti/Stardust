using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IExtendReduceTimeOptimizerService
    {
        /// <summary>
        /// Occurs when [report progress].
        /// </summary>
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        /// <summary>
        /// Executes this service.
        /// </summary>
        /// <param name="optimizers">The optimizers.</param>
        void Execute(IEnumerable<IExtendReduceTimeOptimizer> optimizers);

        /// <summary>
        /// Called when [report progress].
        /// </summary>
        /// <param name="message">The message.</param>
        void OnReportProgress(string message);
    }

    public class ExtendReduceTimeOptimizerService : IExtendReduceTimeOptimizerService
    {
        private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
    	private bool _cancelMe;
		private ResourceOptimizerProgressEventArgs _progressEvent;

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;



        public ExtendReduceTimeOptimizerService(IPeriodValueCalculator periodValueCalculatorForAllSkills)
        {
            _periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
        }

        public void Execute(IEnumerable<IExtendReduceTimeOptimizer> optimizers)
        {
            using (PerformanceOutput.ForOperation("Extending and reduces time for " + optimizers.Count() + " agents"))
            {
	            _progressEvent = null;

                if (_cancelMe)
                    return;

                executeOptimizersWhileActiveFound(optimizers);
            }
        }

        public void OnReportProgress(string message)
        {
        	var handler = ReportProgress;
            if (handler != null)
            {
                var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
                handler(this, args);
                if (args.Cancel)
                    _cancelMe = true;

				if (_progressEvent != null && _progressEvent.UserCancel) return;
				_progressEvent = args;
            }
        }

        private void executeOptimizersWhileActiveFound(IEnumerable<IExtendReduceTimeOptimizer> optimizers)
        {
            IList<IExtendReduceTimeOptimizer> successfulContainers =
                new List<IExtendReduceTimeOptimizer>(optimizers);

            while (successfulContainers.Count > 0)
            {
                IList<IExtendReduceTimeOptimizer> unSuccessfulContainers =
                    shuffleAndExecuteOptimizersInList(successfulContainers);

                if (_cancelMe)
                    break;

				if (_progressEvent != null && _progressEvent.UserCancel)
					break;

                foreach (IExtendReduceTimeOptimizer unSuccessfulContainer in unSuccessfulContainers)
                {
                    successfulContainers.Remove(unSuccessfulContainer);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.ExtendReduceTimeOptimizerService.OnReportProgress(System.String)")]
        private IList<IExtendReduceTimeOptimizer> shuffleAndExecuteOptimizersInList(ICollection<IExtendReduceTimeOptimizer> activeOptimizers)
        {
            IList<IExtendReduceTimeOptimizer> retList = new List<IExtendReduceTimeOptimizer>();
            int executes = 0;
            double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization);
            double newPeriodValue = lastPeriodValue;
            foreach (IExtendReduceTimeOptimizer optimizer in activeOptimizers.GetRandom(activeOptimizers.Count, true))
            {
                bool result = optimizer.Execute();
                executes++;
                if (!result)
                {
                    retList.Add(optimizer);
                }
                else
                {
                    newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization);
                }

                string who = Resources.ExtendingAndReducingTime + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
                string success;
                if (!result)
                {
                    success = " " + Resources.wasNotSuccessful;
                }
                else
                {
                    success = " " + Resources.wasSuccessful;
                }
                string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ")";
                OnReportProgress(who + success + values);

                lastPeriodValue = newPeriodValue;
                if (_cancelMe)
                    break;

				if (_progressEvent != null && _progressEvent.UserCancel)
					break;
            }
            return retList;
        }
    }
}