﻿using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Reporting;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public class ReportSettingsScheduledTimePerActivityPresenter
    {
        private IReportSettingsScheduledTimePerActivityView _view;

        public ReportSettingsScheduledTimePerActivityPresenter(IReportSettingsScheduledTimePerActivityView view)
        {
            _view = view;
        }

        //config settings page for the specific report, hide/show controls etc...
        public void InitializeSettings()
        {
            _view.InitAgentSelector();
            _view.InitActivitiesSelector();
            _view.HideTimeZoneControl();
        }

        //get the model for the specific report
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ReportSettingsScheduledTimePerActivityModel GetSettingsModel()
        {
            ReportSettingsScheduledTimePerActivityModel model = new ReportSettingsScheduledTimePerActivityModel();
            model.Activities = _view.Activities;
            model.Period = _view.Period;
            model.Persons = _view.Persons;
            model.Scenario = _view.Scenario;
            model.TimeZone = _view.TimeZone;

            return model;
        }
    }
}
