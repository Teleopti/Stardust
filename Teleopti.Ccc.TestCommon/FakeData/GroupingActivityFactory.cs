using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for GroupingActivity domain object
    /// </summary>
    public static class GroupingActivityFactory
    {
        /// <summary>
        /// Creates the grouping activity aggregate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="activities">The activities.</param>
        /// <returns></returns>
        public static GroupingActivity CreateGroupingActivityAggregate(string name, IList<Activity> activities)
        {
            GroupingActivity myGroupingActivity = new GroupingActivity(name);
            foreach (Activity activity in activities)
            {
                myGroupingActivity.AddActivity(activity);
            }
            return myGroupingActivity;
        }

        /// <summary>
        /// Creates a grouping activity
        /// </summary>
        /// <param name="name">Name of grouping activity</param>
        /// <returns></returns>
        public static GroupingActivity CreateSimpleGroupingActivity(string name)
        {
            GroupingActivity myGroupingActivity = new GroupingActivity(name);
            return myGroupingActivity;
        }

        /// <summary>
        /// Creates A grouping activity without taking params
        /// </summary>
        public static GroupingActivity CreateSimpleGroupingActivity()
        {
            return CreateSimpleGroupingActivity("Lunch");
        }
    }
}