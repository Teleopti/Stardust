using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
	public interface IBudgetGroupMainViewFactory
	{
		IBudgetGroupMainView Create(IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod, IScenario scenario);
	}
}