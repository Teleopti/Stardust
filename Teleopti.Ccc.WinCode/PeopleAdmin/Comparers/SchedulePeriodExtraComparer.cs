#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the extra property in schedule period data.
    /// </summary>
    public class SchedulePeriodExtraComparer : IComparer<SchedulePeriodModel>
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
            return TimeSpan.Compare(x.Extra, y.Extra);
        }

        #endregion
    }
}
