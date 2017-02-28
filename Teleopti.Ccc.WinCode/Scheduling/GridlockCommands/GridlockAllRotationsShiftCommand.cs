using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllRotationsShiftCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayRotationRestrictionExtractor _scheduleDayRotationExtractor;

        public GridlockAllRotationsShiftCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayRotationRestrictionExtractor scheduleDayRotationExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayRotationExtractor = scheduleDayRotationExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayRotationExtractor.AllRestrictedShifts(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal);
        }
    }
}
