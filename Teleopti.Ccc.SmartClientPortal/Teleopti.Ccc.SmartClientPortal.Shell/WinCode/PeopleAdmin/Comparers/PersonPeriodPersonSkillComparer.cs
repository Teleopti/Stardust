#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the person skills of the person period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 24-07-2008
	/// </remarks>
	public class PersonPeriodPersonSkillComparer : IComparer<PersonPeriodModel>
	{
		#region IComparer<PersonPeriodModel> Members

		/// <summary>
		/// Comparese the person skills data of two objects objects.
		/// </summary>
		/// <param name="x">A Person Period Grid Data object</param>
		/// <param name="y">A Person Period Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonPeriodModel x, PersonPeriodModel y)
		{
			int result = 0;

			if (string.IsNullOrEmpty(x.PersonSkills) && string.IsNullOrEmpty(y.PersonSkills))
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (string.IsNullOrEmpty(x.PersonSkills))
			{
				result = -1;
			}
			else if (string.IsNullOrEmpty(y.PersonSkills))
			{
				result = 1;
			}
			else
			{
				// compares the teminal date of the y with the teminal date of y
				result = string.Compare(x.PersonSkills, y.PersonSkills, StringComparison.CurrentCulture);
			}

			return result;
		}

		#endregion
	}
}
