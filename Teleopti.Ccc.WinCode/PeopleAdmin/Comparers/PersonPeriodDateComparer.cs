﻿#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the period date of the person period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 23-07-2008
	/// </remarks>
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
                result = DateTime.Compare((DateTime)x.PeriodDate, (DateTime)y.PeriodDate);
            }

            return result;
		}

		#endregion
	}
}
