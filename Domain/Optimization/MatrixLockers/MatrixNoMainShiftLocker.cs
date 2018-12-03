using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    public class MatrixNoMainShiftLocker
    {
        private readonly IEnumerable<IScheduleMatrixPro> _scheduleMatrixList;

        public MatrixNoMainShiftLocker(IEnumerable<IScheduleMatrixPro> scheduleMatrixList)
        {
            _scheduleMatrixList = scheduleMatrixList;
        }

        public void Execute()
        {
            foreach (IScheduleMatrixPro scheduleMatrix in _scheduleMatrixList)
            {
                foreach (IScheduleDayPro day in scheduleMatrix.EffectivePeriodDays)
                {
                	IScheduleDay scheduleDay = day.DaySchedulePart();
                	DateOnly dateOnly = day.Day;
					if (!scheduleDay.IsScheduled())
                    {
						scheduleMatrix.LockDay(dateOnly);
                    }
					if (scheduleDay.PersonAbsenceCollection().Length > 0)
                    {
						scheduleMatrix.LockDay(dateOnly);
                    }
                }
            }

        }
    }
}
