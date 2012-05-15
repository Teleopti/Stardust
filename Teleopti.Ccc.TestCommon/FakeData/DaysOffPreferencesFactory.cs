using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class DaysOffPreferencesFactory
    {
        public static IDaysOffPreferences Create()
        {
            return new DaysOffPreferences();
        }

        public static IDaysOffPreferences CreateWithFalseDefaultValues()
        {
            IDaysOffPreferences rules = Create();
            rules.UseConsecutiveDaysOff = false;
            rules.UseConsecutiveWorkdays = false;
            rules.UseDaysOffPerWeek = false;
            rules.KeepFreeWeekendDays = false;
            rules.KeepFreeWeekends = false;
            return rules;
        }

    }
}
