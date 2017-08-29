using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class DayOffFactory
    {
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
