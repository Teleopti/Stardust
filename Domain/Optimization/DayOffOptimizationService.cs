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

        public DayOffOptimizationService(
            IPeriodValueCalculator periodValueCalculator
            )
        {
            _periodValueCalculatorForAllSkills = periodValueCalculator;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void Execute(IEnumerable<IDayOffOptimizerContainer> optimizers)
        {
            using (PerformanceOutput.ForOperation("Optimizing days off for " + optimizers.Count() + " agents"))
            {
                executeOptimizersWhileActiveFound(optimizers);
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
                ResourceOptimizerProgressEventArgs args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
                handler(this, args);
                if (args.Cancel)
                    _cancelMe = true;
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
                IList<IDayOffOptimizerContainer> unSuccessfulContainers =
                    shuffleAndExecuteOptimizersInList(successfulContainers);

                if (_cancelMe)
                    break;

                foreach (IDayOffOptimizerContainer unSuccessfulContainer in unSuccessfulContainers)
                {
                    successfulContainers.Remove(unSuccessfulContainer);
                }
            }
        }

        private IList<IDayOffOptimizerContainer> shuffleAndExecuteOptimizersInList(ICollection<IDayOffOptimizerContainer> activeOptimizers)
        {
            IList<IDayOffOptimizerContainer> retList = new List<IDayOffOptimizerContainer>();
            int executes = 0;
            double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
            double newPeriodValue = lastPeriodValue;
            foreach (IDayOffOptimizerContainer optimizer in activeOptimizers.GetRandom(activeOptimizers.Count, true))
            {
                bool result = optimizer.Execute();
                executes++;
				if (!result)
					retList.Add(optimizer);

				newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				if (lastPeriodValue <= newPeriodValue)
					retList.Add(optimizer);

                string who = Resources.OptimizingDaysOff + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
                string success;
                if (result)
                    success = " " + Resources.wasSuccessful;
                else
                    success = " " + Resources.wasNotSuccessful;
                string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ")";
                OnReportProgress(who + success + values);
                lastPeriodValue = newPeriodValue;
                if (_cancelMe)
                    break;
            }
            return retList;
        }


    }
}
