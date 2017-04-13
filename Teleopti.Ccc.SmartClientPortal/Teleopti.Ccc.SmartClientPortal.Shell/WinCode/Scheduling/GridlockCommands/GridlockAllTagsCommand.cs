using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllTagsCommand : IExecutableCommand
    {
        private readonly IGridlockManager _gridlockManager;
        private readonly IScheduleDayTagExtractor _scheduleDayTagExtractor;

        public GridlockAllTagsCommand(IGridlockManager gridlockManager, IScheduleDayTagExtractor scheduleDayTagExtractor)
        {
            _gridlockManager = gridlockManager;
            _scheduleDayTagExtractor = scheduleDayTagExtractor;
        }

        public void Execute()
        {
            _gridlockManager.AddLock(_scheduleDayTagExtractor.All(), LockType.Normal);
        }
    }
}
