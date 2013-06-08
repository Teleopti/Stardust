﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IMatrixUserLockLocker
    {
        void Execute(IEnumerable<IScheduleDay> scheduleDays, IEnumerable<IScheduleMatrixPro> scheduleMatrixes, DateOnlyPeriod selectedPeriod);
    }

    public class MatrixUserLockLocker : IMatrixUserLockLocker
    {
        private readonly IGridlockManager _gridlockManager;

        public MatrixUserLockLocker(IGridlockManager gridlockManager)
        {
            _gridlockManager = gridlockManager;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(IEnumerable<IScheduleDay> scheduleDays, IEnumerable<IScheduleMatrixPro> scheduleMatrixes, DateOnlyPeriod selectedPeriod)
        {
            foreach (var matrix in scheduleMatrixes)
            {
                var currentPerson = matrix.Person;

                IList<DateOnly> dates = new List<DateOnly>();

                foreach (var scheduleDay in scheduleDays)
                {
                    if (scheduleDay.Person.Equals(currentPerson))
                        dates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
                }

				setUserLockedDaysInMatrix(matrix, selectedPeriod, _gridlockManager);
            }
        }

		private static void setUserLockedDaysInMatrix(IScheduleMatrixPro matrix, DateOnlyPeriod selectedPeriod, IGridlockManager gridlockManager)
        {
            var currentPerson = matrix.Person;

            foreach (var dayPro in matrix.EffectivePeriodDays)
            {
                var day = dayPro.Day;

                if (selectedPeriod.Contains(day))
                    matrix.UnlockPeriod(new DateOnlyPeriod(day, day));

                var locks = gridlockManager.Gridlocks(currentPerson, day);
                if (locks != null && locks.Count != 0)
                    matrix.LockPeriod(new DateOnlyPeriod(day, day));
            }
        }
    }
}
