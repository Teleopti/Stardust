using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for Payload domain object
    /// </summary>
    public static class ActivityFactory
    {
        /// <summary>
        /// Creates an activity aggregate.
        /// </summary>
        /// <returns></returns>
        public static Activity CreateActivity(string name,
                                              Color color)
        {
            Activity ret = new Activity(name);
            ((IActivity)ret).SetId(Guid.NewGuid());
            ret.GroupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity("test");
            ret.DisplayColor = color;
            return ret;
        }

        /// <summary>
        /// Creates an activity aggregate.
        /// </summary>
        /// <returns></returns>
        public static Activity CreateActivity(string name)
        {
            return CreateActivity(name, Color.Green);
        }

        public static IList<IActivity> CreateSomeActivities()
        {
            return new List<IActivity>
                       {
                                       CreateActivity("Phone",Color.Green),    
                                       CreateActivity("Mail",Color.DeepPink),    
                                       CreateActivity("ShortBreak",Color.Firebrick),    
                                       CreateActivity("Lunch",Color.Goldenrod),    
                                       CreateActivity("Handling",Color.DeepSkyBlue),    
                                       };
        }

				public static IActivity ActivityWithId()
				{
					var activity = new Activity("asdf");
					activity.SetId(Guid.NewGuid());
					return activity;
				}
    }
}