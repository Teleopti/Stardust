using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Budgeting.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting
{
	public interface IBudgetGroupMainViewFactory
	{
		IBudgetGroupMainView Create(IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod, IScenario scenario);
	}
}