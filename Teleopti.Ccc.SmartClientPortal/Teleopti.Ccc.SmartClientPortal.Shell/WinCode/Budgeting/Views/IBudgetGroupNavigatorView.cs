using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views
{
    public interface IBudgetGroupNavigatorView
    {
        int BudgetingActionPaneHeight { get; set; }
        BudgetGroupRootModel BudgetGroupRootModel { get; set; }
        IBudgetModel SelectedModel { get; set; }
    }
}