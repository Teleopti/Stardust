using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCode.Grouping
{
    public interface IPeopleNavigator : INavigationPanel
    {
        void AddNew();
        bool SendMessageVisible { get; set; }
    	bool SendMessageEnable { get; set; }
        bool AddNewEnabled { get; set; }
        void FindPeople();
    }
}