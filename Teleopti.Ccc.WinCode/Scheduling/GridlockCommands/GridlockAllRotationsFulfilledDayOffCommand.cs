﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllRotationsFulfilledDayOffCommand : IExecutableCommand
    {
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayRotationRestrictionExtractor _scheduleDayRotationRestrictionExtractor;
        private readonly ICheckerRestriction _restrictionChecker;
        private readonly IGridSchedulesExtractor _schedulesExtractor;

        public GridlockAllRotationsFulfilledDayOffCommand(IGridSchedulesExtractor schedulesExtractor, ICheckerRestriction restrictionChecker, IScheduleDayRotationRestrictionExtractor scheduleDayRotationRestrictionExtractor, IGridlockManager lockManager)
        {
            _lockManager = lockManager;
            _scheduleDayRotationRestrictionExtractor = scheduleDayRotationRestrictionExtractor;
            _restrictionChecker = restrictionChecker;
            _schedulesExtractor = schedulesExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                _restrictionChecker.ScheduleDay = scheduleDay;
                var restrictedDay = _scheduleDayRotationRestrictionExtractor.RestrictionFulfilledDayOff(_restrictionChecker);

                if (restrictedDay != null)
                    restrictedDays.Add(scheduleDay);
            }

            _lockManager.AddLock(restrictedDays, LockType.Normal); 
        }
    }
}
