using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllAvailabilityFulfilledCommand : IExecutableCommand
    {
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayAvailabilityRestrictionExtractor _scheduleDayAvailabilityRestrictionExtractor;
        private readonly ICheckerRestriction _restrictionChecker;
        private readonly IGridSchedulesExtractor _schedulesExtractor;

        public GridlockAllAvailabilityFulfilledCommand(IGridSchedulesExtractor schedulesExtractor, ICheckerRestriction restrictionChecker, IScheduleDayAvailabilityRestrictionExtractor scheduleDayAvailabilityRestrictionExtractor, IGridlockManager lockManager)
        {
            _lockManager = lockManager;
            _scheduleDayAvailabilityRestrictionExtractor = scheduleDayAvailabilityRestrictionExtractor;
            _restrictionChecker = restrictionChecker;
            _schedulesExtractor = schedulesExtractor;
        }

        public void Execute()
        {
            var scheduleDays = _schedulesExtractor.Extract();
            var restrictedDays = new List<IScheduleDay>();

            foreach (var scheduleDay in scheduleDays)
            {
                //_restrictionChecker.ScheduleDay = scheduleDay;
                var restrictedDay = _scheduleDayAvailabilityRestrictionExtractor.RestrictionFulfilled(_restrictionChecker, scheduleDay);

                if (restrictedDay != null)
                    restrictedDays.Add(scheduleDay);
            }

            _lockManager.AddLock(restrictedDays, LockType.Normal); 
        }
    }
}
