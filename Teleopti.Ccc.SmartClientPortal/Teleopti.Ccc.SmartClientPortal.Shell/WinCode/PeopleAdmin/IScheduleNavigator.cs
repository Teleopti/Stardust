using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin
{
    public interface IScheduleNavigator : INavigationPanel
    {
        bool OpenMeetingOverviewEnabled{ get; set; }
        bool AddMeetingOverviewEnabled{ get; set; }
        bool OpenIntradayTodayEnabled { get; set; }
    }
}