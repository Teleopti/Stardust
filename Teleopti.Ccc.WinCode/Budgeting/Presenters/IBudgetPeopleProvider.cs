using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
	public interface IBudgetPeopleProvider
	{
		IEnumerable<IPerson> FindPeopleWithBudgetGroup(IBudgetGroup budgetGroup, DateOnly day);
	}
}