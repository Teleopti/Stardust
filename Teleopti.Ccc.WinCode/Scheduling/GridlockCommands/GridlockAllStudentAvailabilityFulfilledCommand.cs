using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.GridlockCommands
{
    public class GridlockAllStudentAvailabilityFulfilledCommand : IExecutableCommand
    {
        private readonly IGridlockManager _lockManager;
        private readonly IScheduleDayStudentAvailabilityRestrictionExtractor _scheduleDayStudentAvailabilityRestrictionExtractor;
        private readonly ICheckerRestriction _restrictionChecker;
        private readonly IGridSchedulesExtractor _schedulesExtractor;

        public GridlockAllStudentAvailabilityFulfilledCommand(IGridSchedulesExtractor schedulesExtractor, ICheckerRestriction restrictionChecker, IScheduleDayStudentAvailabilityRestrictionExtractor scheduleDayStudentAvailabilityRestrictionExtractor, IGridlockManager lockManager)
        {
            _lockManager = lockManager;
            _scheduleDayStudentAvailabilityRestrictionExtractor = scheduleDayStudentAvailabilityRestrictionExtractor;
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
                var restrictedDay = _scheduleDayStudentAvailabilityRestrictionExtractor.RestrictionFulfilled(_restrictionChecker, scheduleDay);

                if (restrictedDay != null)
                    restrictedDays.Add(scheduleDay);
            }

            _lockManager.AddLock(restrictedDays, LockType.Normal); 
        }
    }
}
