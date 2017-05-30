using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    public class MatrixPersonalShiftLocker
    {
        private readonly IEnumerable<IScheduleMatrixPro> _scheduleMatrixList;

        public MatrixPersonalShiftLocker(IEnumerable<IScheduleMatrixPro> scheduleMatrixList)
        {
            _scheduleMatrixList = scheduleMatrixList;
        }

        public void Execute()
        {
            foreach (var matrixPro in _scheduleMatrixList)
            {
                foreach (var scheduleDayPro in matrixPro.UnlockedDays)
                {
                    var personAssignment = scheduleDayPro.DaySchedulePart().PersonAssignment();

                    if (personAssignment != null && personAssignment.PersonalActivities().Any())
                    {
                        matrixPro.LockDay(scheduleDayPro.Day);
                    }
                }
            }    
        }
    }
}
