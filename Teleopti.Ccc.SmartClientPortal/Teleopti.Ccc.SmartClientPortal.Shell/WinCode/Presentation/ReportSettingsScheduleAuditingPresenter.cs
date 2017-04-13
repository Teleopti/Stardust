
namespace Teleopti.Ccc.WinCode.Presentation
{
    public class ReportSettingsScheduleAuditingPresenter
    {
        private readonly IReportSettingsScheduleAuditingView _view;

        public ReportSettingsScheduleAuditingPresenter(IReportSettingsScheduleAuditingView view)
        {
            _view = view;
        }

        public ReportSettingsScheduleAuditingModel GetSettingsModel
        {
            get
            {
                var settingsModel = new ReportSettingsScheduleAuditingModel();
                foreach (var modifier in _view.ModifiedBy)
                {
                    settingsModel.AddModifier(modifier);
                }

                settingsModel.ChangePeriod = _view.ChangePeriod;
                settingsModel.SchedulePeriod = _view.SchedulePeriod;

                settingsModel.ChangePeriodDisplay = _view.ChangePeriodDisplay;
                settingsModel.SchedulePeriodDisplay = _view.SchedulePeriodDisplay;

                foreach (var agent in _view.Agents)
                {
                    settingsModel.AddAgent(agent);
                }

                return settingsModel;
            }
        }

        public void InitializeSettings()
        {
            _view.InitUserSelector();
            _view.InitPersonSelector();
            _view.SetDateControlTexts();
        }
    }
}
