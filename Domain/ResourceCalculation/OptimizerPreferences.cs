using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class OptimizerPreferences : IOptimizerPreferences
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptimizerPreferences"/> class.
        /// </summary>
        /// <param name="originalOptimizerPreferences">The resource re optimizer user preferences.</param>
        public OptimizerPreferences(IOptimizerOriginalPreferences originalOptimizerPreferences)
        {
            InParameter.NotNull("userDefinedOptimizerPreferences", originalOptimizerPreferences);
            OriginalOptimizerPreferences = originalOptimizerPreferences;
            IterationOptimizerPreferences = new OptimizerIterationPreferences();
            if (OriginalOptimizerPreferences.SchedulingOptions != null)
            {
                IterationOptimizerPreferences.IterationSchedulingOptions =
                    (ISchedulingOptions)OriginalOptimizerPreferences.SchedulingOptions.Clone();
            }
        }

        public IOptimizerOriginalPreferences OriginalOptimizerPreferences
        {
            get; set;
        }

        public IOptimizerIterationPreferences IterationOptimizerPreferences
        {
            get; set;
        }
    }
}
