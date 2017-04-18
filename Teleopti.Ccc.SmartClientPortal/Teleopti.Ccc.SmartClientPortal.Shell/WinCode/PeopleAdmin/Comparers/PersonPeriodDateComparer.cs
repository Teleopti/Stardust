using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the period date of the person period data.
	/// </summary>
	public class PersonPeriodDateComparer : IComparer<PersonPeriodModel>
	{
		#region IComparer<PersonPeriodModel> Members

		/// <summary>
		/// Comparese the person period date of two objects objects.
		/// </summary>
		/// <param name="x">A Person Period Grid Data object</param>
		/// <param name="y">A Person Period Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonPeriodModel x, PersonPeriodModel y)
		{
            int result = 0;

            if (x.PeriodDate == null && y.PeriodDate == null)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.PeriodDate == null)
            {
                result = -1;
            }
            else if (y.PeriodDate == null)
            {
                result = 1;
            }
            else
            {
                // compares the teminal date of the y with the teminal date of y
                result = DateTime.Compare(x.PeriodDate.Value.Date,y.PeriodDate.Value.Date);
            }

            return result;
		}

		#endregion
	}
}
