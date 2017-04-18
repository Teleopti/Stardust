#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the Cultire data.
    /// </summary>
    /// <remarks>
    /// Created By: madhurangap
    /// Created Date: 29-07-2008
    /// </remarks>
    public class PeopleAdminCultureComparer : IComparer<PersonGeneralModel>
    {
        #region IComparer<PersonGeneralModel> Members

        /// <summary>
        /// Comparese the language of two objects objects.
        /// </summary>
        /// <param name="x">A People Admin Grid Data object</param>
        /// <param name="y">A People Admin Grid Data object</param>
        /// <returns>Result of the comparisom</returns>
        public int Compare(PersonGeneralModel x, PersonGeneralModel y)
        {
            int result = 0;

            if (x.CultureInfo.Id == 0 && y.CultureInfo.Id == 0)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.CultureInfo.Id == 0)
            {
                result = -1;
            }
            else if (y.CultureInfo.Id == 0)
            {
                result = 1;
            }
            else
            {
                // compares the language of the y with the language of y
                result = string.Compare(x.CultureInfo.DisplayName,
                    y.CultureInfo.DisplayName, StringComparison.CurrentCulture);
            }

            return result;
        }

        #endregion

    }
}
