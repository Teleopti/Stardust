using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	public class PersonPeriodBudgetGroupComparer : IComparer<PersonPeriodModel>
	{
		public int Compare(PersonPeriodModel x, PersonPeriodModel y)
		{
			int result = 0;

			if (x.BudgetGroup == null && y.BudgetGroup == null)
			{
			}
			else if (x.BudgetGroup == null)
			{
				result = -1;
			}
			else if (y.BudgetGroup == null)
			{
				result = 1;
			}
			else
			{
				result = string.Compare(x.BudgetGroup.Name, y.BudgetGroup.Name, StringComparison.CurrentCulture);
			}

			return result;
		}
	}
}
