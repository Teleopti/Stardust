using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
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
		    using (PerformanceOutput.ForOperation("Optimizing days off for " + successfulContainers.Count + " agents"))
		    {
			    string values = " ";
			    int loops = 0;
				while (successfulContainers.Count > 0)
				{
					loops ++;
					double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
					IList<IDayOffOptimizerContainer> unSuccessfulContainers = new List<IDayOffOptimizerContainer>();
					int executes = 0;					
					foreach (IDayOffOptimizerContainer optimizer in successfulContainers.GetRandom(successfulContainers.Count, true))
					{
						if (cancel) return;

						bool result = optimizer.Execute();
						executes++;
						if (!result)
							unSuccessfulContainers.Add(optimizer);

						string progress = Resources.OptimizingDaysOff + Resources.Colon + "(" + loops + ")(" + successfulContainers.Count + ")" + executes + " ";
						string who = optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
						string success = result ? " " + Resources.wasSuccessful : " " + Resources.wasNotSuccessful;						
						
						var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, progress + values + who + success, () => cancel = true));
						if (cancel || progressResult.ShouldCancel) return;					
					}

					double newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
					values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ") ";

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
