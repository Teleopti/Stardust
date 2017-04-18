using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public interface IBudgetPeopleProvider
	{
		IEnumerable<IPerson> FindPeopleWithBudgetGroup(IBudgetGroup budgetGroup, DateOnly day);
	}
}