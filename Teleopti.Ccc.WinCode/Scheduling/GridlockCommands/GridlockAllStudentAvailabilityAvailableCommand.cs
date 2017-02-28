using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllStudentAvailabilityAvailableCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayStudentAvailabilityRestrictionExtractor _scheduleDayStudentAvailabilityExtractor;

        public GridlockAllStudentAvailabilityAvailableCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayStudentAvailabilityExtractor = scheduleDayStudentAvailabilityExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayStudentAvailabilityExtractor.AllAvailable(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal);
        }
    }
}
