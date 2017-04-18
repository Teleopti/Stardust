using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
	public class ReportSettingsScheduleTimeVersusTargetTimePresenter
	{
		private readonly IReportSettingsScheduleTimeVersusTargetTimeView _view;

		public ReportSettingsScheduleTimeVersusTargetTimePresenter(IReportSettingsScheduleTimeVersusTargetTimeView view)
		{
			_view = view;
		}

		public void InitializeSettings()
		{
			_view.InitAgentSelector();
		}

		public ReportSettingsScheduleTimeVersusTargetTimeModel GetSettingsModel
		{
			get
			{
				var model = new ReportSettingsScheduleTimeVersusTargetTimeModel();
				model.Period = _view.Period;
				model.SetPersons(_view.Persons);
				model.Scenario = _view.Scenario;

				return model;
			}
		}
	}
}
