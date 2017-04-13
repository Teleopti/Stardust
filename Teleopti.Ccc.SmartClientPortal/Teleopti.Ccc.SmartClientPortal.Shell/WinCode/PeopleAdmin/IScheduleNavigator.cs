using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public interface IScheduleNavigator : INavigationPanel
    {
        bool OpenMeetingOverviewEnabled{ get; set; }
        bool AddMeetingOverviewEnabled{ get; set; }
        bool OpenIntradayTodayEnabled { get; set; }
    }
}