#region Imports

using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the days off property the schedule period data.
    /// </summary>
    /// <remarks>
    /// Created By: madhurangap
    /// Created Date: 25-07-2008
    /// </remarks>
    public class SchedulePeriodDaysOffComparer : IComparer<SchedulePeriodModel>
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

            if ((!x.DaysOff.HasValue) && (!y.DaysOff.HasValue))
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (!x.DaysOff.HasValue)
            {
                result = -1;
            }
            else if (!y.DaysOff.HasValue)
            {
                result = 1;
            }
            else
            {
                if (x.DaysOff < y.DaysOff)
                {
                    result = -1;
                }
                if (y.DaysOff < x.DaysOff)
                {
                    result = 1;
                }
            }

            return result;
        }

        #endregion
    }
}
