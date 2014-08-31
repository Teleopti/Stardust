using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    /// <summary>
    /// Locks the days with overtime in the matrix.
    /// </summary>
    public interface IMatrixOvertimeLocker
    {
        void Execute();
    }

    public class MatrixOvertimeLocker : IMatrixOvertimeLocker
    {
        private readonly IList<IScheduleMatrixPro> _matrixList;

        public MatrixOvertimeLocker(IList<IScheduleMatrixPro> matrixList)
        {
            _matrixList = matrixList;
        }

        public void Execute()
        {
            foreach (var matrixPro in _matrixList)
            {
                foreach (var scheduleDayPro in matrixPro.UnlockedDays)
                {
                    var personAssignment = scheduleDayPro.DaySchedulePart().PersonAssignment();

                    if (personAssignment != null && personAssignment.OvertimeActivities().Any())
                    {
                        matrixPro.LockPeriod(new DateOnlyPeriod(scheduleDayPro.Day, scheduleDayPro.Day));
                    }
                }    
            }    
        }
    }
}
