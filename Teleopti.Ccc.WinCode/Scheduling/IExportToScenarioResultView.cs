using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IExportToScenarioResultView
    {
        void CloseForm();
        void SetScenarioText(string headerText);
        void SetAgentText(string agentText);
        void SetWarningText(IEnumerable<ExportToScenarioWarningData> validationWarnings);
        void DisableBodyText();
    }
}