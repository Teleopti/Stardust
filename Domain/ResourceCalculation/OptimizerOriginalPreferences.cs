using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class OptimizerOriginalPreferences : IOptimizerOriginalPreferences
    {
        private readonly ISchedulingOptions _userDefinedSchedulingOptions;

        public OptimizerOriginalPreferences()
        {
            _userDefinedSchedulingOptions = new SchedulingOptions();
        }

        public OptimizerOriginalPreferences(
            ISchedulingOptions userDefinedSchedulingOptions)
        {
            _userDefinedSchedulingOptions = userDefinedSchedulingOptions;
        }

        public ISchedulingOptions SchedulingOptions
        {
            get { return _userDefinedSchedulingOptions; }
        }
    }
}
