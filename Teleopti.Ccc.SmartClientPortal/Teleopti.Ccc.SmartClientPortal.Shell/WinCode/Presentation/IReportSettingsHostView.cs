using Teleopti.Ccc.WinCode.Reporting;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public interface IReportSettingsHostView
    {
        void ShowSettings(ReportDetail reportDetail);
        void SetupFromScheduler(ReportDetail reportDetail);
	    void ShowSpinningProgress(bool show);
        void SetHeaderText(string text);
        void Unfold();
        void Fold();
        bool Unfolded();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        IReportSettingsScheduledTimePerActivityView GetSettingsForScheduledTimePerActivityReport();
        void ReportHeaderCheckRightToLeft();
        void AddSettingsForScheduledTimePerActivityReport(IReportSettingsScheduledTimePerActivityView settingsScheduledTimePerActivityView);
        void DisableShowSettings();

        ReportSettingsScheduledTimePerActivityModel ScheduleTimePerActivitySettingsModel { get; }
        IReportSettingsScheduleAuditingView GetSettingsForScheduleAuditingReport { get; }
        void AddSettingsForScheduleAuditingReport(IReportSettingsScheduleAuditingView settingsScheduleAuditingView);

    	IReportSettingsScheduleTimeVersusTargetTimeView GetSettingsForScheduleTimeVersusTargetTimeReport { get; }
    	void AddSettingsForScheduleTimeVersusTargetTimeReport(IReportSettingsScheduleTimeVersusTargetTimeView settingsScheduleTimeVersusTargetTimeView);
    	void SetReportFunctionCode(string functionCode);
    }
}
