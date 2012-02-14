using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{

    public class ScheduleMatrixOriginalStateContainer : IScheduleMatrixOriginalStateContainer
    {
        private readonly IScheduleMatrixPro _matrix;
        private readonly IScheduleDayEquator _scheduleDayEquator;
        private readonly IDictionary<DateOnly, IScheduleDay> _oldPeriodDaysState;
        public bool StillAlive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ScheduleMatrixOriginalStateContainer(IScheduleMatrixPro matrix, IScheduleDayEquator scheduleDayEquator)
        {
            _matrix = matrix;
            _scheduleDayEquator = scheduleDayEquator;
            _oldPeriodDaysState = new Dictionary<DateOnly, IScheduleDay>();
            foreach (IScheduleDayPro scheduleDayPro in matrix.EffectivePeriodDays)
            {
                _oldPeriodDaysState.Add(scheduleDayPro.Day, (IScheduleDay)scheduleDayPro.DaySchedulePart().Clone());
            }
            StillAlive = true;

        }

        public IScheduleMatrixPro ScheduleMatrix
        {
            get { return _matrix; }
        }

        public IDictionary<DateOnly, IScheduleDay> OldPeriodDaysState
        {
            get { return _oldPeriodDaysState; }
        }

        public bool IsFullyScheduled()
        {
            foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                if (!scheduleDayPro.DaySchedulePart().IsScheduled())
                    return false;
            }

            return true;
        }


        public int CountChangedDayOffs()
        {
            int result = 0;
            foreach (IScheduleDayPro day in _matrix.EffectivePeriodDays)
            {
                DateOnly key = day.Day;
                IScheduleDay originalDay = OldPeriodDaysState[key];
                IScheduleDay currentDay = day.DaySchedulePart();
                if (!_scheduleDayEquator.DayOffEquals(originalDay, currentDay))
                    result++;
            }
            return result;
        }

        public int CountChangedWorkShifts()
        {
            int result = 0;
            foreach (IScheduleDayPro day in _matrix.EffectivePeriodDays)
            {
                DateOnly key = day.Day;
                if (WorkShiftChanged(key))
                    result++;
            }
            return result;
        }

        public bool WorkShiftChanged(DateOnly dateOnly)
        {
            IScheduleDay originalDay = OldPeriodDaysState[dateOnly];
            IScheduleDay currentDay = _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
            if (!_scheduleDayEquator.MainShiftEquals(originalDay, currentDay))
                return true;

            return false;
        }
    }
}