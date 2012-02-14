using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Reporting;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public interface IReportScheduledTimePerActivityView
    {
        ReportSettingsScheduledTimePerActivityModel ScheduleTimePerActivitySettingsModel { get; }
        IUnitOfWork TheUnitOfWork { get; }
    }
}