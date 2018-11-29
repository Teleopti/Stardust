using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
    public interface IReportSettingsScheduledTimePerActivityView
    {
        void InitializeSettings();
        IScenario Scenario { get; }
        DateOnlyPeriod Period { get; }
        IList<IPerson> Persons { get; }
        TimeZoneInfo TimeZone { get; }
        IList<IActivity> Activities { get; }
        ReportSettingsScheduledTimePerActivityModel ScheduleTimePerActivitySettingsModel { get; }
        void InitAgentSelector();
        void InitActivitiesSelector();
        void HideTimeZoneControl();
    }
}
