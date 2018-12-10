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

		public virtual IReportSettingsScheduleTimeVersusTargetTimeView SettingsForScheduleTimeVersusTargetTimeReport { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void ShowSettings(ReportDetail reportDetail)
        {
            switch (reportDetail.FunctionPath)
            {          
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


		public ReportSettingsScheduleTimeVersusTargetTimeModel GetModelForScheduleTimeVersusTargetTimeReport
		{
			get {return SettingsForScheduleTimeVersusTargetTimeReport == null ? null : SettingsForScheduleTimeVersusTargetTimeReport.ScheduleTimeVersusTargetTimeModel;}
		}
    }
}
