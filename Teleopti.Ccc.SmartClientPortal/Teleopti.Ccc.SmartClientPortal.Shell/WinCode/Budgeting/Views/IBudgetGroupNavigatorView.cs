using Teleopti.Ccc.WinCode.Budgeting.Models;

namespace Teleopti.Ccc.WinCode.Budgeting.Views
{
    public interface IBudgetGroupNavigatorView
    {
        int BudgetingActionPaneHeight { get; set; }
        BudgetGroupRootModel BudgetGroupRootModel { get; set; }
        IBudgetModel SelectedModel { get; set; }
    }
}