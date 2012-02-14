#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the contract schedulTeam data of the person period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 29-07-2008
	/// </remarks>
    public class PersonPeriodTeamComparer : IComparer<PersonPeriodModel>
    {
        #region IComparer<PersonPeriodModel> Members

        /// <summary>
        /// Comparese the contract schedule of two objects objects.
        /// </summary>
        /// <param name="x">A Person Period Grid Data object</param>
        /// <param name="y">A Person Period Grid Data object</param>
        /// <returns>Result of the comparisom</returns>
        public int Compare(PersonPeriodModel x, PersonPeriodModel y)
        {
            int result = 0;

            if (x.Team == null && y.Team == null)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.Team == null)
            {
                result = -1;
            }
            else if (y.Team == null)
            {
                result = 1;
            }
            else
            {
                // compares the teminal date of the y with the teminal date of y
                result = string.Compare(x.Team.Description.Name, y.Team.Description.Name, StringComparison.CurrentCulture);
            }

            return result;
        }

        #endregion
    }
}
