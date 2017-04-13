using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AuditHistory
{
    public class NextPageCommand : IExecutableCommand, ICanExecute
    {
        private readonly IAuditHistoryModel _model;

        public NextPageCommand(IAuditHistoryModel auditHistoryModel)
        {
            _model = auditHistoryModel;
        }

        public void Execute()
        {
            if(CanExecute())
                _model.Earlier();
        }

        public bool CanExecute()
        {
            if (_model.CurrentPage < _model.NumberOfPages)
                return true;

            return false;
        }
    }
}