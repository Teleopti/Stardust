namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
    public interface IReportSettingsHostView
    {
        void ShowSettings(ReportDetail reportDetail);
	    void ShowSpinningProgress(bool show);
        void SetHeaderText(string text);
        void Unfold();
        void Fold();
        bool Unfolded();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        void ReportHeaderCheckRightToLeft();
        void DisableShowSettings();
    	IReportSettingsScheduleTimeVersusTargetTimeView GetSettingsForScheduleTimeVersusTargetTimeReport { get; }
    	void AddSettingsForScheduleTimeVersusTargetTimeReport(IReportSettingsScheduleTimeVersusTargetTimeView settingsScheduleTimeVersusTargetTimeView);
    	void SetReportFunctionCode(string functionCode);
    }
}
