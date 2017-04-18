using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllRotationsCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayRotationRestrictionExtractor _scheduleDayRotationExtractor;

        public GridlockAllRotationsCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayRotationExtractor = scheduleDayRotationExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayRotationExtractor.AllRestrictedDays(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal);
        }
    }
}
