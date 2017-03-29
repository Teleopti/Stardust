﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
    /// <summary>
    /// Locks the days with full day meeting.
    /// </summary>
    public interface IMatrixMeetingDayLocker
    {
        void Execute();
    }

    public class MatrixMeetingDayLocker : IMatrixMeetingDayLocker
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
