using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the ExternalLogOnName of the person period data.
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 2008-08-14
    /// </remarks>
    public class PersonPeriodExternalLogOnNameComparer : IComparer<PersonPeriodModel>
    {
        /// <summary>
        /// Comparese the person skills data of two objects objects.
        /// </summary>
        /// <param name="x">A Person Period Grid Data object</param>
        /// <param name="y">A Person Period Grid Data object</param>
        /// <returns>Result of the comparisom</returns>
        public int Compare(PersonPeriodModel x, PersonPeriodModel y)
        {
            int result = 0;

            if (string.IsNullOrEmpty(x.ExternalLogOnNames) && string.IsNullOrEmpty(y.ExternalLogOnNames))
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (string.IsNullOrEmpty(x.ExternalLogOnNames))
            {
                result = -1;
            }
            else if (string.IsNullOrEmpty(y.ExternalLogOnNames))
            {
                result = 1;
            }
            else
            {
                result = string.Compare(x.ExternalLogOnNames, y.ExternalLogOnNames, StringComparison.CurrentCulture);
            }

            return result;
        }
    }
}
