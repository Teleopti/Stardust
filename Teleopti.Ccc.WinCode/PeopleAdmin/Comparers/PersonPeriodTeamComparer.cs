using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the contract schedulTeam data of the person period data.
	/// </summary>
    public class PersonPeriodTeamComparer : IComparer<PersonPeriodModel>
    {
		/// <summary>
        /// Comparese the contract schedule of two objects objects.
        /// </summary>
        /// <param name="x">A Person Period Grid Data object</param>
        /// <param name="y">A Person Period Grid Data object</param>
        /// <returns>Result of the comparisom</returns>
        public int Compare(PersonPeriodModel x, PersonPeriodModel y)
        {
            int result = 0;

            if (x.SiteTeam == null && y.SiteTeam == null)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.SiteTeam == null)
            {
                result = -1;
            }
            else if (y.SiteTeam == null)
            {
                result = 1;
            }
            else
            {
                // compares the teminal date of the y with the teminal date of y
                result = string.Compare(x.SiteTeam.Team.Description.Name, y.SiteTeam.Team.Description.Name, StringComparison.CurrentCulture);
            }

            return result;
        }
    }
}
