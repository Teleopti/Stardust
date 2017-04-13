using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AuditHistory
{
    public class PreviousPageCommand : IExecutableCommand, ICanExecute
    {
        private readonly IAuditHistoryModel _model;

        public PreviousPageCommand(IAuditHistoryModel auditHistoryModel)
        {
            _model = auditHistoryModel;
        }

        public void Execute()
        {
            if(CanExecute())
                _model.Later();
        }

        public bool CanExecute()
        {
            if (_model.CurrentPage > 1)
                return true;

            return false;
        }
    }
}