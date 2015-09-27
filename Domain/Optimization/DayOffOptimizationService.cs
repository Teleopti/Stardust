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
    	
        public DayOffOptimizationService(IPeriodValueCalculator periodValueCalculator)
        {
            _periodValueCalculatorForAllSkills = periodValueCalculator;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void Execute(IEnumerable<IDayOffOptimizerContainer> optimizers)
        {
			var successfulContainers = new List<IDayOffOptimizerContainer>(optimizers);
	        var cancel = false;
		    using (PerformanceOutput.ForOperation("Optimizing days off for " + successfulContainers.Count() + " agents"))
            {
				while (successfulContainers.Count > 0)
				{
					IList<IDayOffOptimizerContainer> unSuccessfulContainers = new List<IDayOffOptimizerContainer>();
					int executes = 0;
					double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
					foreach (IDayOffOptimizerContainer optimizer in successfulContainers.GetRandom(successfulContainers.Count, true))
					{
						if (cancel) return;

						bool result = optimizer.Execute();
						executes++;
						if (!result)
							unSuccessfulContainers.Add(optimizer);

						double newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);

						string progress = Resources.OptimizingDaysOff + Resources.Colon + "(" + successfulContainers.Count + ")" + executes + " ";
						string who = optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
						string success = result ? " " + Resources.wasSuccessful : " " + Resources.wasNotSuccessful;
						string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ") ";
						
						var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, progress + values + who + success, () => cancel = true));
						if (cancel || progressResult.ShouldCancel) return;

						lastPeriodValue = newPeriodValue;
					}

					foreach (IDayOffOptimizerContainer unSuccessfulContainer in unSuccessfulContainers)
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
		            return new CancelSignal{ShouldCancel = true};
            }
			return new CancelSignal();
        }
    }
}
