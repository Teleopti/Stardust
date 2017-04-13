using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Budgeting.Views
{
    public interface IBudgetGroupTabView
    {
        DialogResult AskToCommitChanges();
        void OnSave(object obj);
        void NotifyBudgetDaysUpdatedByOthers();
        void ShowMonthView();
        void ShowWeekView();
        void ShowDayView();
    }
}