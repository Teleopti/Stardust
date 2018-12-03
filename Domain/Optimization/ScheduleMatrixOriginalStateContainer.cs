using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{

    public class ScheduleMatrixOriginalStateContainer : IScheduleMatrixOriginalStateContainer
    {
        private readonly IScheduleMatrixPro _matrix;
        private readonly IScheduleDayEquator _scheduleDayEquator;
        private readonly IDictionary<DateOnly, IScheduleDay> _oldPeriodDaysState;
        private int _originalNumberOfDaysOff;
    	private TimeSpan? _originalWorkTime;
        public bool StillAlive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ScheduleMatrixOriginalStateContainer(IScheduleMatrixPro matrix, IScheduleDayEquator scheduleDayEquator)
        {
            _matrix = matrix;
            _scheduleDayEquator = scheduleDayEquator;
            _oldPeriodDaysState = new Dictionary<DateOnly, IScheduleDay>();
            foreach (IScheduleDayPro scheduleDayPro in matrix.EffectivePeriodDays)
            {
                var scheduleDay = (IScheduleDay) scheduleDayPro.DaySchedulePart().Clone();
                var significantPart = scheduleDay.SignificantPart();

                if (significantPart == SchedulePartView.DayOff)
                    _originalNumberOfDaysOff++;

                _oldPeriodDaysState.Add(scheduleDayPro.Day, scheduleDay);
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
	        return _matrix.IsFullyScheduled();
        }


        public double ChangedDayOffsPercent()
        {
            int changedDaysOff = 0;
            foreach (IScheduleDayPro day in _matrix.EffectivePeriodDays)
            {
                DateOnly key = day.Day;
                IScheduleDay originalDay = OldPeriodDaysState[key];
                IScheduleDay currentDay = day.DaySchedulePart();
                if (!_scheduleDayEquator.DayOffEquals(originalDay, currentDay))
                    changedDaysOff++;
            }

            if (_originalNumberOfDaysOff == 0)
                return 0;

            return (double)changedDaysOff/_originalNumberOfDaysOff;
        }

        public bool WorkShiftChanged(DateOnly dateOnly)
        {
            IScheduleDay originalDay = OldPeriodDaysState[dateOnly];
            IScheduleDay currentDay = _matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart();
            if (!_scheduleDayEquator.MainShiftEquals(originalDay, currentDay))
                return true;

            return false;
        }

		public TimeSpan OriginalWorkTime()
		{
			if (!_originalWorkTime.HasValue)
			{
				_originalWorkTime = TimeSpan.Zero;
				foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
				{
					IScheduleDay oldDay = _oldPeriodDaysState[scheduleDayPro.Day];
					_originalWorkTime = _originalWorkTime.Value.Add(oldDay.ProjectionService().CreateProjection().ContractTime());
				}
			}

			return _originalWorkTime.Value;
		}
    }
}