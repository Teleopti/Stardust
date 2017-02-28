using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface IShowExportMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class ShowExportMeetingCommand :IShowExportMeetingCommand
    {
        private readonly IExportMeetingPresenter _exportMeetingPresenter;
        private readonly IExportableScenarioProvider _exportableScenarioProvider;

        public ShowExportMeetingCommand(IExportMeetingPresenter exportMeetingPresenter, IExportableScenarioProvider exportableScenarioProvider)
        {
            _exportMeetingPresenter = exportMeetingPresenter;
            _exportableScenarioProvider = exportableScenarioProvider;
        }

        public void Execute()
        {
            if(CanExecute())
                _exportMeetingPresenter.Show();
        }

        public bool CanExecute()
        {
            return _exportableScenarioProvider.AllowedScenarios().Count > 0;
        }
    }
}