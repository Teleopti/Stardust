using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class ActivityFactory
    {
        public static Activity CreateActivity(string name,
                                              Color color)
        {
            Activity ret = new Activity(name);
            ((IActivity)ret).SetId(Guid.NewGuid());
            ret.DisplayColor = color;
            return ret;
        }

        public static Activity CreateActivity(string name)
        {
            return CreateActivity(name, Color.Green);
        }
    }
}