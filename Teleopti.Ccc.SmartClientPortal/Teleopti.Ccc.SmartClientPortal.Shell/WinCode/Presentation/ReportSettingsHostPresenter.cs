using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
    public class ReportSettingsHostPresenter
    {
        private readonly IReportSettingsHostView _view;
       
        public ReportSettingsHostPresenter(IReportSettingsHostView view)
        {
            _view = view;
        }

        public virtual IReportSettingsScheduledTimePerActivityView SettingsForScheduledTimePerActivityReport { get; private set; }
        public virtual IReportSettingsScheduleAuditingView SettingsForScheduleAuditingReport { get; private set; }
		public virtual IReportSettingsScheduleTimeVersusTargetTimeView SettingsForScheduleTimeVersusTargetTimeReport { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void ShowSettings(ReportDetail reportDetail)
        {
            switch (reportDetail.FunctionPath)
            {
                case DefinedRaptorApplicationFunctionPaths.ScheduledTimePerActivityReport:
                    showSettingsForScheduledTimePerActivity();
                    break;
                case DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailReport:
                    //NEW_AUDIT
                    showSettingsForScheduleAuditing();
                    break;
				case DefinedRaptorApplicationFunctionPaths.ScheduleTimeVersusTargetTimeReport:
					ShowSettingsForScheduleTimeVersusTargetTime();
            		break;
                default: 
                    return;
            }

            _view.ReportHeaderCheckRightToLeft();
            _view.SetHeaderText(reportDetail.DisplayName);
            _view.SetReportFunctionCode(reportDetail.FunctionCode);
            _view.Unfold();
        }

        //NEW_AUDIT
        private void showSettingsForScheduleAuditing()
        {
            var reportSettings = _view.GetSettingsForScheduleAuditingReport;
            _view.AddSettingsForScheduleAuditingReport(reportSettings);
            SettingsForScheduleAuditingReport = reportSettings;
            reportSettings.InitializeSettings();
        }

        private void showSettingsForScheduledTimePerActivity()
        {
            IReportSettingsScheduledTimePerActivityView reportSettings =_view.GetSettingsForScheduledTimePerActivityReport();
            _view.AddSettingsForScheduledTimePerActivityReport(reportSettings);
            SettingsForScheduledTimePerActivityReport = reportSettings;
            reportSettings.InitializeSettings();
        }

		private void ShowSettingsForScheduleTimeVersusTargetTime()
		{
			IReportSettingsScheduleTimeVersusTargetTimeView reportSettings = _view.GetSettingsForScheduleTimeVersusTargetTimeReport;
			_view.AddSettingsForScheduleTimeVersusTargetTimeReport(reportSettings);
			SettingsForScheduleTimeVersusTargetTimeReport = reportSettings;
			reportSettings.InitializeSettings();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void HideSettingsAndSetReportHeader(ReportDetail reportDetail)
        {
            _view.SetReportFunctionCode(reportDetail.FunctionCode);
            _view.DisableShowSettings();
            _view.ReportHeaderCheckRightToLeft();
            _view.SetHeaderText(reportDetail.DisplayName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ReportSettingsScheduledTimePerActivityModel GetModelForScheduledTimePerActivityReport()
        {
            if (SettingsForScheduledTimePerActivityReport == null)
                return null;

            return SettingsForScheduledTimePerActivityReport.ScheduleTimePerActivitySettingsModel;
        }

        public ReportSettingsScheduleAuditingModel GetModelForScheduleAuditingReport
        {
            get
            {
                if (SettingsForScheduleAuditingReport == null)
                    return null;

                return SettingsForScheduleAuditingReport.ScheduleAuditingSettingsModel;
            }
        }

		public ReportSettingsScheduleTimeVersusTargetTimeModel GetModelForScheduleTimeVersusTargetTimeReport
		{
			get {return SettingsForScheduleTimeVersusTargetTimeReport == null ? null : SettingsForScheduleTimeVersusTargetTimeReport.ScheduleTimeVersusTargetTimeModel;}
		}
    }
}
