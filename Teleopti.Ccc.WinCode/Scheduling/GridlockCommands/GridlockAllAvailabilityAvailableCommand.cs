﻿using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllAvailabilityAvailableCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayAvailabilityRestrictionExtractor _scheduleDayAvailabilityExtractor;

        public GridlockAllAvailabilityAvailableCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayAvailabilityExtractor = scheduleDayAvailabilityExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayAvailabilityExtractor.AllAvailable(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal);
        }
    }
}
