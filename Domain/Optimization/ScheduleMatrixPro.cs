using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleMatrixPro : IScheduleMatrixPro
    {
	    private readonly IDictionary<DateOnly, IScheduleDayPro> _effectivePeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
	    private readonly IDictionary<DateOnly, IScheduleDayPro> _fullWeeksPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
	    private readonly IDictionary<DateOnly, IScheduleDayPro> _outerWeeksPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
	    private readonly IDictionary<DateOnly, IScheduleDayPro> _weekBeforeOuterPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
        private readonly IDictionary<DateOnly, IScheduleDayPro> _weekAfterOuterPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
        private readonly IDictionary<DateOnly, IScheduleDayPro> _unLockedDays = new Dictionary<DateOnly, IScheduleDayPro>();

	    /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDayPro"/> class.
        /// </summary>
        /// <param name="stateHolder">The state holder.</param>
        /// <param name="periodCreator">The period creator.</param>
        /// <param name="schedulePeriod">The schedule period.</param>
        public ScheduleMatrixPro(ISchedulingResultStateHolder stateHolder, IFullWeekOuterWeekPeriodCreator periodCreator, IVirtualSchedulePeriod schedulePeriod)
        {
            Person = periodCreator.Person;
            SchedulePeriod = schedulePeriod;
            ActiveScheduleRange = stateHolder.Schedules[Person];
            createScheduleDays(periodCreator);
        }

		public ScheduleMatrixPro(IScheduleRange activeScheduleRange, IFullWeekOuterWeekPeriodCreator periodCreator, IVirtualSchedulePeriod schedulePeriod)
		{
			Person = periodCreator.Person;
			SchedulePeriod = schedulePeriod;
			ActiveScheduleRange = activeScheduleRange;
			createScheduleDays(periodCreator);
		}

	    public bool IsDayLocked(DateOnly date)
	    {
		    return !_unLockedDays.ContainsKey(date);
	    }

        public IPerson Person { get; }

	    public IScheduleDayPro[] FullWeeksPeriodDays => _fullWeeksPeriodDays.Values.ToArray();

	    public IScheduleDayPro[] OuterWeeksPeriodDays => _outerWeeksPeriodDays.Values.ToArray();

	    public IScheduleDayPro[] WeekBeforeOuterPeriodDays => _weekBeforeOuterPeriodDays.Values.ToArray();

	    public IScheduleDayPro[] WeekAfterOuterPeriodDays => _weekAfterOuterPeriodDays.Values.ToArray();

	    public IDictionary<DateOnly, IScheduleDayPro> OuterWeeksPeriodDictionary => _outerWeeksPeriodDays;

	    public IScheduleDayPro[] EffectivePeriodDays => _effectivePeriodDays.Values.ToArray();

	    public IScheduleDayPro[]  UnlockedDays => _unLockedDays.Values.ToArray();

	    public void UnlockPeriod(DateOnlyPeriod period)
		{
			foreach (var dateOnly in period.DayCollection())
			{
				IScheduleDayPro scheduleDayPro;
				if (!_effectivePeriodDays.TryGetValue(dateOnly, out scheduleDayPro))
					throw new ArgumentOutOfRangeException(nameof(period));
				if (!_unLockedDays.ContainsKey(dateOnly))
					_unLockedDays.Add(dateOnly, scheduleDayPro);
			}
		}

		public void LockDay(DateOnly date)
		{
			_unLockedDays.Remove(date);
		}

		public IScheduleDayPro GetScheduleDayByKey(DateOnly dateOnly)
        {
            IScheduleDayPro ret;
            _outerWeeksPeriodDays.TryGetValue(dateOnly, out ret);
            return ret;
        }

        public IVirtualSchedulePeriod SchedulePeriod { get; }

	    public IScheduleRange ActiveScheduleRange { get; }
	    public bool IsFullyScheduled()
	    {
		    foreach (var scheduleDayPro in EffectivePeriodDays)
		    {
			    if (!scheduleDayPro.DaySchedulePart().IsScheduled())
				    return false;
		    }

		    return true;
		}

	    /// <summary>
        /// Creates the schedule days.
        /// </summary>
        /// <param name="periodCreator">The period creator.</param>
        private void createScheduleDays(IFullWeekOuterWeekPeriodCreator periodCreator)
        {
            var outerDays = periodCreator.OuterWeekPeriod().DayCollection();
            var fullWeekPeriod = periodCreator.FullWeekPeriod();
            var effectivePeriod = periodCreator.EffectivePeriod();
            foreach (DateOnly dateOnly in outerDays)
            {
                IScheduleDayPro scheduleDayPro = new ScheduleDayPro(dateOnly, this);
                _outerWeeksPeriodDays.Add(dateOnly, scheduleDayPro);

                if (fullWeekPeriod.Contains(dateOnly))
                    _fullWeeksPeriodDays.Add(dateOnly, scheduleDayPro);

                if (effectivePeriod.Contains(dateOnly))
                    _effectivePeriodDays.Add(dateOnly, scheduleDayPro);

                int count = outerDays.Count;
                if(outerDays.IndexOf(dateOnly) < count - 7)
                {
                    _weekBeforeOuterPeriodDays.Add(dateOnly, scheduleDayPro);
                }
                if (outerDays.IndexOf(dateOnly) > 6)
                {
                    _weekAfterOuterPeriodDays.Add(dateOnly, scheduleDayPro);
                }
            }
        }
    }
}