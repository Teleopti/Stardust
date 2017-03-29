using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IExtendReduceDaysOffOptimizerService
    {
        /// <summary>
        /// Occurs when [report progress].
        /// </summary>
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        /// <summary>
        /// Executes this service.
        /// </summary>
        /// <param name="optimizers">The optimizers.</param>
        void Execute(IEnumerable<IExtendReduceDaysOffOptimizer> optimizers);
    }

    public class ExtendReduceDaysOffOptimizerService : IExtendReduceDaysOffOptimizerService
    {
         private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
    	
        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public ExtendReduceDaysOffOptimizerService(IPeriodValueCalculator periodValueCalculatorForAllSkills)
        {
            _periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
        }

	    public void Execute(IEnumerable<IExtendReduceDaysOffOptimizer> optimizers)
	    {
		    var successfulContainers = optimizers.ToList();
		    var cancel = false;
		    using (
			    PerformanceOutput.ForOperation("Extending and reduces time for " + successfulContainers.Count + " agents"))
		    {
			    while (successfulContainers.Count > 0)
			    {
				    IList<IExtendReduceDaysOffOptimizer> unSuccessfulContainers = new List<IExtendReduceDaysOffOptimizer>();
				    int executes = 0;
				    double lastPeriodValue =
					    _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
				    double newPeriodValue = lastPeriodValue;
				    foreach (
					    IExtendReduceDaysOffOptimizer optimizer in successfulContainers.GetRandom(successfulContainers.Count, true))
				    {
						if (cancel) return;

					    bool result = optimizer.Execute();
					    executes++;
					    if (!result)
					    {
						    unSuccessfulContainers.Add(optimizer);
					    }
					    else
					    {
						    newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
					    }

					    string who = Resources.ExtendingAndReducingDaysoff + Resources.Colon + "(" + successfulContainers.Count + ")" +
					                 executes + " " + optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
					    string success = !result ? " " + Resources.wasNotSuccessful : " " + Resources.wasSuccessful;
					    string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ")";
						var progressFeedback = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, who + success + values, 100, () => cancel = true));
						if (cancel || progressFeedback.ShouldCancel) return;

					    lastPeriodValue = newPeriodValue;
				    }

				    foreach (IExtendReduceDaysOffOptimizer unSuccessfulContainer in unSuccessfulContainers)
				    {
					    successfulContainers.Remove(unSuccessfulContainer);
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
	            if (args.Cancel)
		            return new CancelSignal {ShouldCancel = true};
            }
			return new CancelSignal();
        }
    }
}