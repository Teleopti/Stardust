using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class OptimizerOriginalPreferences : IOptimizerOriginalPreferences
    {
        private readonly IDayOffPlannerRules _userDefinedDayOffPlannerRules;
        private readonly ISchedulingOptions _userDefinedSchedulingOptions;

        public OptimizerOriginalPreferences()
        {
            _userDefinedDayOffPlannerRules = new DayOffPlannerRules();
            _userDefinedSchedulingOptions = new SchedulingOptions();
        }

        public OptimizerOriginalPreferences(
            IDayOffPlannerRules userDefinedDayOffPlannerRules,
            ISchedulingOptions userDefinedSchedulingOptions)
        {
            _userDefinedDayOffPlannerRules = userDefinedDayOffPlannerRules;
            _userDefinedSchedulingOptions = userDefinedSchedulingOptions;
        }

        public IDayOffPlannerRules DayOffPlannerRules
        { 
            get { return _userDefinedDayOffPlannerRules; }
        }

        public ISchedulingOptions SchedulingOptions
        {
            get { return _userDefinedSchedulingOptions; }
        }
    }
}
