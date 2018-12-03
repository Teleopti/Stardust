using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class EmptyPersonAccountBalanceCalculator : IPersonAccountBalanceCalculator
    {
        private readonly IAbsence _absence;

        public EmptyPersonAccountBalanceCalculator(IAbsence absence)
        {
            _absence = absence;
        }

        public bool CheckBalance(IScheduleRange scheduleRange, DateOnlyPeriod period)
        {
            return (_absence.Tracker == null);
        }
    }
}