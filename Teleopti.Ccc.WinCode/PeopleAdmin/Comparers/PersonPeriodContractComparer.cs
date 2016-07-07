#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the contract data of the person period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 23-07-2008
	/// </remarks>
	public class PersonPeriodContractComparer : IComparer<PersonPeriodModel>
	{
		#region IComparer<PersonPeriodModel> Members

		/// <summary>
		/// Comparese the contract data of two objects objects.
		/// </summary>
		/// <param name="x">A Person Period Grid Data object</param>
		/// <param name="y">A Person Period Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonPeriodModel x, PersonPeriodModel y)
		{
			int result = 0;

			if (x.Contract == null && y.Contract == null)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.Contract == null)
			{
				result = -1;
			}
			else if (y.Contract == null)
			{
				result = 1;
			}
			else
			{
				// compares the teminal date of the y with the teminal date of y
				result = string.Compare(x.Contract.Description.Name, y.Contract.Description.Name, StringComparison.CurrentCulture);
			}

			return result;
		}

		#endregion
	}
}
