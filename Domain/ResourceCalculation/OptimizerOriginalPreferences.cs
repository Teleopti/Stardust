using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class OptimizerOriginalPreferences : IOptimizerOriginalPreferences
    {
        private readonly SchedulingOptions _userDefinedSchedulingOptions;

        public OptimizerOriginalPreferences()
        {
            _userDefinedSchedulingOptions = new SchedulingOptions();
        }

        public OptimizerOriginalPreferences(SchedulingOptions userDefinedSchedulingOptions)
        {
            _userDefinedSchedulingOptions = userDefinedSchedulingOptions;
        }

        public SchedulingOptions SchedulingOptions => _userDefinedSchedulingOptions;
    }
}
