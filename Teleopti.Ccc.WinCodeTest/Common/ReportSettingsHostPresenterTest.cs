using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    public class ReportSettingsHostPresenterForTest : ReportSettingsHostPresenter
    {
        private readonly IReportSettingsScheduledTimePerActivityView _settingsForScheduledTimePerActivity;
        private readonly IReportSettingsScheduleAuditingView _settingsForScheduleAuditing;

        public ReportSettingsHostPresenterForTest(IReportSettingsHostView view, IReportSettingsScheduledTimePerActivityView settingsForScheduledTimePerActivity)
            : base(view)
        {
            _settingsForScheduledTimePerActivity = settingsForScheduledTimePerActivity;
        }

        public ReportSettingsHostPresenterForTest(IReportSettingsHostView view, IReportSettingsScheduleAuditingView settingsForScheduleAuditing)
            : base(view)
        {
            _settingsForScheduleAuditing = settingsForScheduleAuditing;
        }

        public override IReportSettingsScheduledTimePerActivityView SettingsForScheduledTimePerActivityReport
        {
            get
            {
                return _settingsForScheduledTimePerActivity;
            }
        }

        public override IReportSettingsScheduleAuditingView SettingsForScheduleAuditingReport
        {
            get
            {
                return _settingsForScheduleAuditing;
            }
        }
    }
}
