using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    public class MatrixMeetingDayLocker
    {
        private readonly IEnumerable<IScheduleMatrixPro> _scheduleMatrixList;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "scheduleMatrixList")]
        public MatrixMeetingDayLocker(IEnumerable<IScheduleMatrixPro> scheduleMatrixList)
        {
            _scheduleMatrixList = scheduleMatrixList;
        }

        public void Execute()
        {
            foreach (IScheduleMatrixPro scheduleMatrix in _scheduleMatrixList)
            {
                foreach (var scheduleDayPro in scheduleMatrix.UnlockedDays)
                {
                        if (scheduleDayPro.DaySchedulePart().PersonMeetingCollection().Count > 0)
                        {
                            scheduleMatrix.LockDay(scheduleDayPro.Day);
                        }
                }    
            }
        }
    }
}
