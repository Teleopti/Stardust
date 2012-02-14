using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class OptimizerOriginalPreferences : IOptimizerOriginalPreferences
    {
        private readonly IDayOffPlannerRules _userDefinedDayOffPlannerRules;
        private readonly IOptimizerAdvancedPreferences _userDefinedOptimizerAdvancedPreferences;
        private readonly ISchedulingOptions _userDefinedSchedulingOptions;

        public OptimizerOriginalPreferences()
        {
            _userDefinedDayOffPlannerRules = new DayOffPlannerRules();
            _userDefinedOptimizerAdvancedPreferences = new OptimizerAdvancedPreferences();
            _userDefinedSchedulingOptions = new SchedulingOptions();
        }

        public OptimizerOriginalPreferences(
            IDayOffPlannerRules userDefinedDayOffPlannerRules,
            IOptimizerAdvancedPreferences userDefinedOptimizerAdvancedPreferences,
            ISchedulingOptions userDefinedSchedulingOptions)
        {
            _userDefinedDayOffPlannerRules = userDefinedDayOffPlannerRules;
            _userDefinedOptimizerAdvancedPreferences = userDefinedOptimizerAdvancedPreferences;
            _userDefinedSchedulingOptions = userDefinedSchedulingOptions;
        }

        public IDayOffPlannerRules DayOffPlannerRules
        { 
            get { return _userDefinedDayOffPlannerRules; }
        }

        public IOptimizerAdvancedPreferences AdvancedPreferences
        {
            get { return _userDefinedOptimizerAdvancedPreferences; }
        }

        public ISchedulingOptions SchedulingOptions
        {
            get { return _userDefinedSchedulingOptions; }
        }
    }
}
