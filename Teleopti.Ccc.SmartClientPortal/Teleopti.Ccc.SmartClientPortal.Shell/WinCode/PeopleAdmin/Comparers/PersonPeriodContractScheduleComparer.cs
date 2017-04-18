#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the contract schedul of the person period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 24-07-2008
	/// </remarks>
	public class PersonPeriodContractScheduleComparer : IComparer<PersonPeriodModel>
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

			if (x.ContractSchedule == null && y.ContractSchedule == null)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.ContractSchedule == null)
			{
				result = -1;
			}
			else if (y.ContractSchedule == null)
			{
				result = 1;
			}
			else
			{
				// compares the teminal date of the y with the teminal date of y
				result = string.Compare(x.ContractSchedule.Description.Name, y.ContractSchedule.Description.Name, StringComparison.CurrentCulture);
			}

			return result;
		}

		#endregion
	}
}
