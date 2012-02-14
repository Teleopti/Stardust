#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Represents the PErson Account Date Comparer.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-08
    /// </remarks>
    public class PersonAccountDateComparer : IComparer<IPersonAccountModel>
    {


        #region Methods - Instance Member - IComparer<IPersonAccountModel> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, 
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        public int Compare(IPersonAccountModel x, IPersonAccountModel y)
        {
            int result = 0;

            if (x.CurrentAccount == null && y.CurrentAccount == null)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.CurrentAccount == null)
            {
                result = -1;
            }
            else if (y.CurrentAccount == null)
            {
                result = 1;
            }
            else
            {
                // Compares the values as strings.
                result = DateTime.Compare((DateTime)x.AccountDate, (DateTime)y.AccountDate);
            }

            return result;
        }

        #endregion
    }
}
