using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    public class MatrixOvertimeLocker
    {
        private readonly IEnumerable<IScheduleMatrixPro> _matrixList;

        public MatrixOvertimeLocker(IEnumerable<IScheduleMatrixPro> matrixList)
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
                        matrixPro.LockDay(scheduleDayPro.Day);
                    }
                }    
            }    
        }
    }
}
