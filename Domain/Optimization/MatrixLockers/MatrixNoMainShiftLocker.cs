using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    /// <summary>
    /// Locks the days with full day, or part day Absence or days that is not scheduled
    /// </summary>
    public interface IMatrixNoMainShiftLocker
    {
        void Execute();
    }

    public class MatrixNoMainShiftLocker : IMatrixNoMainShiftLocker
    {
        private readonly IList<IScheduleMatrixPro> _scheduleMatrixList;

        public MatrixNoMainShiftLocker(IList<IScheduleMatrixPro> scheduleMatrixList)
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
						scheduleMatrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
                    }
					if (scheduleDay.PersonAbsenceCollection().Count > 0)
                    {
						scheduleMatrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
                    }
                }
            }

        }
    }
}
