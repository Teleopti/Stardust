using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands 
{
    public class GridlockAllPreferencesDayOffCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayPreferenceRestrictionExtractor _scheduleDayPreferenceRestrictionExtractor;

        public GridlockAllPreferencesDayOffCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayPreferenceRestrictionExtractor = scheduleDayPreferenceRestrictionExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayPreferenceRestrictionExtractor.AllRestrictedDayOffs(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal); 
        }
    }
}
