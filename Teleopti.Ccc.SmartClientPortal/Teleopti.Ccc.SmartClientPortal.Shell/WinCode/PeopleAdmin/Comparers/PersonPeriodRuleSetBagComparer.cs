using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the rule set bag of the person period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 25-07-2008
	/// </remarks>
	public class PersonPeriodRuleSetBagComparer : IComparer<PersonPeriodModel>
	{
		/// <summary>
		/// Comparese the rule set bag of two objects objects.
		/// </summary>
		/// <param name="x">A Person Period Grid Data object</param>
		/// <param name="y">A Person Period Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonPeriodModel x, PersonPeriodModel y)
		{
			int result = 0;

			if (x.RuleSetBag == null && y.RuleSetBag == null)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.RuleSetBag == null)
			{
				result = -1;
			}
			else if (y.RuleSetBag == null)
			{
				result = 1;
			}
			else
			{
				// compares the teminal date of the y with the teminal date of y
				result = string.Compare(x.RuleSetBag.Description.Name, y.RuleSetBag.Description.Name, StringComparison.CurrentCulture);
			}

			return result;
		}
	}
}
