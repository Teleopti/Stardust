using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    /// <summary>
    /// Locks the days with personal shift.
    /// </summary>
    public interface IMatrixPersonalShiftLocker
    {
        void Execute();
    }

    public class MatrixPersonalShiftLocker : IMatrixPersonalShiftLocker
    {
        private readonly IList<IScheduleMatrixPro> _scheduleMatrixList;

        public MatrixPersonalShiftLocker(IList<IScheduleMatrixPro> scheduleMatrixList)
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
                        matrixPro.LockPeriod(new DateOnlyPeriod(scheduleDayPro.Day, scheduleDayPro.Day));
                    }
                }
            }    
        }
    }
}
