using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DayOffOptimizationService : IDayOffOptimizationService
    {
        private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
    	private bool _cancelMe;
	    private ResourceOptimizerProgressEventArgs _progressEvent;

        public DayOffOptimizationService(
            IPeriodValueCalculator periodValueCalculator
            )
        {
            _periodValueCalculatorForAllSkills = periodValueCalculator;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void Execute(IEnumerable<IDayOffOptimizerContainer> optimizers)
        {
			_progressEvent = null;
            using (PerformanceOutput.ForOperation("Optimizing days off for " + optimizers.Count() + " agents"))
            {
                executeOptimizersWhileActiveFound(optimizers);
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
        private void executeOptimizersWhileActiveFound(IEnumerable<IDayOffOptimizerContainer> optimizers)
        {
            IList<IDayOffOptimizerContainer> successfulContainers =
                new List<IDayOffOptimizerContainer>(optimizers);

            while (successfulContainers.Count > 0)
            {
                IEnumerable<IDayOffOptimizerContainer> unSuccessfulContainers =
                    shuffleAndExecuteOptimizersInList(successfulContainers);

                if (_cancelMe)
                    break;

				if (_progressEvent != null && _progressEvent.UserCancel) 
					break;

                foreach (IDayOffOptimizerContainer unSuccessfulContainer in unSuccessfulContainers)
                {
                    successfulContainers.Remove(unSuccessfulContainer);
                }
            }
        }

        private IEnumerable<IDayOffOptimizerContainer> shuffleAndExecuteOptimizersInList(ICollection<IDayOffOptimizerContainer> activeOptimizers)
        {
            IList<IDayOffOptimizerContainer> retList = new List<IDayOffOptimizerContainer>();
            int executes = 0;
            double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
        	foreach (IDayOffOptimizerContainer optimizer in activeOptimizers.GetRandom(activeOptimizers.Count, true))
            {
                bool result = optimizer.Execute();
                executes++;
				if (!result)
					retList.Add(optimizer);

				double newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);

            	string progress = Resources.OptimizingDaysOff + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " ";
                string who = optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
                string success;
                if (result)
                    success = " " + Resources.wasSuccessful;
                else
                    success = " " + Resources.wasNotSuccessful;
                string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ") ";
                OnReportProgress(progress + values + who + success);
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
