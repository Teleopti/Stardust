using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class ActivityFactory
    {
        public static IActivity CreateActivity(string name,
                                              Color color)
        {
            var ret = new Activity(name).WithId();
            ret.DisplayColor = color;
            return ret;
        }

        public static IActivity CreateActivity(string name)
        {
            return CreateActivity(name, Color.Green);
        }
    }
}