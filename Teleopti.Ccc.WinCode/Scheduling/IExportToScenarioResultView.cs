using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.WinCode.Scheduling
{
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