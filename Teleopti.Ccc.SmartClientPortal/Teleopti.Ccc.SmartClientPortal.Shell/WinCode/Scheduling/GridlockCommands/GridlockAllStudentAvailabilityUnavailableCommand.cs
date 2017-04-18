using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllStudentAvailabilityUnavailableCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayStudentAvailabilityRestrictionExtractor _scheduleDayStudentAvailabilityExtractor;

        public GridlockAllStudentAvailabilityUnavailableCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayStudentAvailabilityExtractor = scheduleDayStudentAvailabilityExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayStudentAvailabilityExtractor.AllUnavailable(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal);
        }
    }
}
