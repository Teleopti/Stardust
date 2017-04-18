using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping
{
    public interface IPeopleNavigator : INavigationPanel
    {
        bool SendMessageVisible { get; set; }
    	bool SendMessageEnable { get; set; }
        bool AddNewEnabled { get; set; }
        void FindPeople();
    }
}