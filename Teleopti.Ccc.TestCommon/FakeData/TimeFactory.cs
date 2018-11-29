

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for Time related domain objects
    /// </summary>
    public static class TimeFactory
    {
        /// <summary>
        /// Creates the time period.
        /// </summary>
        /// <param name="startHour">The start hour.</param>
        /// <param name="endHour">The end hour.</param>
        /// <returns></returns>
        public static TimePeriod CreateTimePeriod(int startHour, int endHour)
        {
            return new TimePeriod(startHour,0, endHour,0);
        }
    }
}