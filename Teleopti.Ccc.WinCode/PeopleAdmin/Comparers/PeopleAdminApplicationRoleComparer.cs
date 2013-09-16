using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the roles data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 21-07-2008
	/// </remarks>
	public class PeopleAdminApplicationRoleComparer : IComparer<PersonGeneralModel>
	{
		/// <summary>
		/// Comparese the roels of two objects objects.
		/// </summary>
		/// <param name="x">A People Admin Grid Data object</param>
		/// <param name="y">A People Admin Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonGeneralModel x, PersonGeneralModel y)
		{
            int result = 0;

            if (string.IsNullOrEmpty(x.Roles) && string.IsNullOrEmpty(y.Roles))
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (string.IsNullOrEmpty(x.Roles))
            {
                result = -1;
            }
            else if (string.IsNullOrEmpty(y.Roles))
            {
                result = 1;
            }
            else
            {
                result = string.Compare(x.Roles, y.Roles, StringComparison.CurrentCulture);
            }

            return result;
		}
	}
}
