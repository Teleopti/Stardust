using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views
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