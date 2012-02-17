using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface ISchedulingOptionsSyncronizer
    {
        void SyncronizeSchedulingOption(
            IOptimizationPreferences optimizationPreferences,
            ISchedulingOptions schedulingOptions);
    }

    public class SchedulingOptionsSyncronizer : ISchedulingOptionsSyncronizer
    {
        public void SyncronizeSchedulingOption(
            IOptimizationPreferences optimizationPreferences, 
            ISchedulingOptions schedulingOptions)
        {
            
        }
    }
}
