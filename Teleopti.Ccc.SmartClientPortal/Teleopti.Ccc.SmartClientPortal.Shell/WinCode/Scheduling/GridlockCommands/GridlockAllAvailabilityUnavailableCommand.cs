using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllAvailabilityUnavailableCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayAvailabilityRestrictionExtractor _scheduleDayAvailabilityExtractor;

        public GridlockAllAvailabilityUnavailableCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayAvailabilityExtractor = scheduleDayAvailabilityExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayAvailabilityExtractor.AllUnavailable(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal);
        }
    }
}
