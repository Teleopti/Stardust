using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface ISchedulingOptionsSynchronizer
    {
        void SynchronizeSchedulingOption(
            IOptimizationPreferences optimizationPreferences,
            ISchedulingOptions schedulingOptions);
    }

    public class SchedulingOptionsSynchronizer : ISchedulingOptionsSynchronizer
    {
        public void SynchronizeSchedulingOption(
            IOptimizationPreferences optimizationPreferences, 
            ISchedulingOptions schedulingOptions)
        {
            
        }
    }
}
