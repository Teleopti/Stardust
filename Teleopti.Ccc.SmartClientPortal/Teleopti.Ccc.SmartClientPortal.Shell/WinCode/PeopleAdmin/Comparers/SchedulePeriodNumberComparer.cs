#region Imports

using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the Number of the schedule period data.
    /// </summary>
    /// <remarks>
    /// Created By: madhurangap
    /// Created Date: 30-07-2008
    /// </remarks>
    public class SchedulePeriodNumberComparer : IComparer<SchedulePeriodModel>
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

            if (x.Number == -1 && y.Number == -1)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.Number == -1)
            {
                result = -1;
            }
            else if (y.Number == -1)
            {
                result = 1;
            }
            else
            {
                if (x.Number < y.Number)
                {
                    result = -1;
                }
                if (x.Number > y.Number)
                {
                    result = 1;
                }
            }

            return result;
        }

        #endregion
    }
}
