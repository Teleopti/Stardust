using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Factory for DayOff
    /// </summary>
    /// 
    public static class DayOffFactory
    {
        /// <summary>
        /// Creates a DayOff
        /// </summary>
        /// <returns></returns>
        public static DayOffTemplate CreateDayOff()
        {
            Description desc = new Description("My day off");

            return new DayOffTemplate(desc);
        }

        public static DayOffTemplate CreateDayOff(Description description)
        {
            return new DayOffTemplate(description);
        }
    }
}
