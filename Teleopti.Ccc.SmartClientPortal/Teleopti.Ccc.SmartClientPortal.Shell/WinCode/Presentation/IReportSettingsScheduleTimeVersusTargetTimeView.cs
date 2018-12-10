using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
	public interface IReportSettingsScheduleTimeVersusTargetTimeView
	{
		void InitializeSettings();
		//void InitializeGroupings();
		void InitAgentSelector();
		//ReportGroupPageSelectorModel GroupPage { get; }
		IList<IPerson> Persons { get; }
		IScenario Scenario { get; }
		DateOnlyPeriod Period { get; }
		ReportSettingsScheduleTimeVersusTargetTimeModel ScheduleTimeVersusTargetTimeModel { get; }
	}
}
