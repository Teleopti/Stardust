using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.GridlockCommands
{
    public class GridlockTagCommand : IExecutableCommand
    {
        private readonly IGridlockManager _gridlockManager;
        private readonly IScheduleDayTagExtractor _scheduleDayTagExtractor;
        private readonly IScheduleTag _tag;

        public GridlockTagCommand(IGridlockManager gridlockManager, IScheduleDayTagExtractor scheduleDayTagExtractor, IScheduleTag tag)
        {
            _gridlockManager = gridlockManager;
            _scheduleDayTagExtractor = scheduleDayTagExtractor;
            _tag = tag;
        }

        public void Execute()
        {
            _gridlockManager.AddLock(_scheduleDayTagExtractor.Tag(_tag), LockType.Normal);
        }
    }
}
