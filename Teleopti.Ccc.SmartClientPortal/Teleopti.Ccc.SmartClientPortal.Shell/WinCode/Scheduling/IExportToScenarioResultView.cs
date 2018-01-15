using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_PrepareToRemoveExportSchedule_46576)]
	public interface IExportToScenarioResultView
	{
		void CloseForm();
		void SetScenarioText(string headerText);
		void SetAgentText(string agentText);
		void SetWarningText(IEnumerable<ExportToScenarioWarningData> validationWarnings);
		void DisableBodyText();
		void ShowDataSourceException(DataSourceException exception);
		void DisableInteractions();
		void EnableInteractions();
	}
}