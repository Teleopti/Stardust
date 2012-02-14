using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class OptimizerIterationPreferences : IOptimizerIterationPreferences
    {

        public IOptimizerAdvancedPreferences IterationAdvancedPreferences
        {
            get; set;
        }

        public IDayOffPlannerRules IterationDayOffPlannerRules
        {
            get; set;
        }

        public IterationOperationOption IterationOperation
        {
            get; set;
        }

        public ISchedulingOptions IterationSchedulingOptions
        {
            get; set;
        }
    }
}
