﻿using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllPreferencesCommand : IExecutableCommand
    {
        private readonly IGridSchedulesExtractor _schedulesExtractor;
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayPreferenceRestrictionExtractor _scheduleDayPreferenceRestrictionExtractor;

        public GridlockAllPreferencesCommand(IGridSchedulesExtractor schedulesExtractor, IScheduleDayPreferenceRestrictionExtractor scheduleDayPreferenceRestrictionExtractor, IGridlockManager lockManager)
        {
            _schedulesExtractor = schedulesExtractor;
            _lockManager = lockManager;
            _scheduleDayPreferenceRestrictionExtractor = scheduleDayPreferenceRestrictionExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = _scheduleDayPreferenceRestrictionExtractor.AllRestrictedDays(scheduleDays);

            _lockManager.AddLock(restrictedDays, LockType.Normal);   
        }
    }
}
