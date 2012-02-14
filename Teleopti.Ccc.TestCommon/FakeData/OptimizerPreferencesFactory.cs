using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creates IOptimizerPreferences for testing purposes;
    /// </summary>
    public static class OptimizerPreferencesFactory
    {
        public static IOptimizerOriginalPreferences Create()
        {
            ISchedulingOptions userDefinedSchedulingOptions = new SchedulingOptions();
            userDefinedSchedulingOptions.UseMinimumPersons = false;
            userDefinedSchedulingOptions.UseMaximumPersons = false;
            userDefinedSchedulingOptions.UsePreferences = false;
            userDefinedSchedulingOptions.UseRotations = false;
            userDefinedSchedulingOptions.UseAvailability = false;
            userDefinedSchedulingOptions.UseStudentAvailability = false;

            IDayOffPlannerRules userDefinedDayOffPlannerRules = new DayOffPlannerRules();
            IOptimizerAdvancedPreferences userDefinedOptimizerAdvancedPreferences = new OptimizerAdvancedPreferences();
            IOptimizerOriginalPreferences optimizerOriginalPreferences =
                new OptimizerOriginalPreferences(userDefinedDayOffPlannerRules, userDefinedOptimizerAdvancedPreferences,
                                                 userDefinedSchedulingOptions);



            return optimizerOriginalPreferences;
        }
    }
}
