#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the part time percentage of the person period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 25-07-2008
	/// </remarks>
	public class PersonPeriodPartTimePercentageComparer: IComparer<PersonPeriodModel>
	{

		#region IComparer<PersonPeriodModel> Members

		/// <summary>
		/// Comparese the part time percentage of two objects objects.
		/// </summary>
		/// <param name="x">A Person Period Grid Data object</param>
		/// <param name="y">A Person Period Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonPeriodModel x, PersonPeriodModel y)
		{
			int result = 0;

			if (x.PartTimePercentage == null && y.PartTimePercentage == null)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.PartTimePercentage == null)
			{
				result = -1;
			}
			else if (y.PartTimePercentage == null)
			{
				result = 1;
			}
			else
			{
				// compares the teminal date of the y with the teminal date of y
				result = string.Compare(x.PartTimePercentage.Description.Name, y.PartTimePercentage.Description.Name, StringComparison.CurrentCulture);
			}

			return result;
		}

		#endregion
	}
}
