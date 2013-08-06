using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

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
