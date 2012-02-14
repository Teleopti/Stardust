using System;

namespace Teleopti.Ccc.WinCode.Shifts.Views
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
