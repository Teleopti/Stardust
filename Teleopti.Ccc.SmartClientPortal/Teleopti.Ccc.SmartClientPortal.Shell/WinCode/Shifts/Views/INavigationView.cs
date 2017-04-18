using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views
{
    public interface INavigationView
    {
        event EventHandler<EventArgs> Refresh;

        void ChangeGridView(ShiftCreatorViewType viewType);

        void ForceRefresh();

        void UpdateTreeIcons();

        void AssignRuleSet();
    }
}
