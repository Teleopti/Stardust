#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the average work time of the schedule period data.
    /// </summary>
    /// <remarks>
    /// Created By: madhurangap
    /// Created Date: 30-07-2008
    /// </remarks>
    public class SchedulePeriodAverageWorkTimePerDayComparer : IComparer<SchedulePeriodModel>
    {
        #region IComparer<SchedulePeriodModel> Members

        /// <summary>
        /// Comparese the start date of two objects objects.
        /// </summary>
        /// <param name="x">A Person Period Grid Data object</param>
        /// <param name="y">A Person Period Grid Data object</param>
        /// <returns>Result of the comparisom</returns>
        public int Compare(SchedulePeriodModel x, SchedulePeriodModel y)
        {
            int result = 0;

            if (x.AverageWorkTimePerDay == TimeSpan.Zero && y.AverageWorkTimePerDay == TimeSpan.Zero)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.AverageWorkTimePerDay == TimeSpan.Zero)
            {
                result = -1;
            }
            else if (y.AverageWorkTimePerDay == TimeSpan.Zero)
            {
                result = 1;
            }
            else
            {
                // compares the teminal date of the y with the teminal date of y
                result = TimeSpan.Compare(x.AverageWorkTimePerDay, y.AverageWorkTimePerDay);
            }

            return result;
        }

        #endregion
    }
}
