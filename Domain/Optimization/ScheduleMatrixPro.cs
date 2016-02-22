using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleMatrixPro : IScheduleMatrixPro
    {
	    private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IPerson _person;
        private readonly IVirtualSchedulePeriod _schedulePeriod;
		private readonly IDictionary<DateOnly, IScheduleDayPro> _effectivePeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
	    private readonly IDictionary<DateOnly, IScheduleDayPro> _fullWeeksPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
	    private readonly IDictionary<DateOnly, IScheduleDayPro> _outerWeeksPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
	    private readonly IDictionary<DateOnly, IScheduleDayPro> _weekBeforeOuterPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
        private readonly IDictionary<DateOnly,IScheduleDayPro> _weekAfterOuterPeriodDays = new Dictionary<DateOnly, IScheduleDayPro>();
        private readonly IDictionary<DateOnly, IScheduleDayPro> _unLockedDays = new Dictionary<DateOnly, IScheduleDayPro>();
        private readonly IScheduleRange _activeScheduleRange;

	    /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDayPro"/> class.
        /// </summary>
        /// <param name="stateHolder">The state holder.</param>
        /// <param name="periodCreator">The period creator.</param>
        /// <param name="schedulePeriod">The schedule period.</param>
        public ScheduleMatrixPro(ISchedulingResultStateHolder stateHolder, IFullWeekOuterWeekPeriodCreator periodCreator, IVirtualSchedulePeriod schedulePeriod)
        {
            _stateHolder = stateHolder;
            _person = periodCreator.Person;
            _schedulePeriod = schedulePeriod;
            _activeScheduleRange = stateHolder.Schedules[_person];
            createScheduleDays(periodCreator);
        }

		public ScheduleMatrixPro(IScheduleRange activeScheduleRange, IFullWeekOuterWeekPeriodCreator periodCreator, IVirtualSchedulePeriod schedulePeriod)
		{
			_stateHolder = null;
			_person = periodCreator.Person;
			_schedulePeriod = schedulePeriod;
			_activeScheduleRange = activeScheduleRange;
			createScheduleDays(periodCreator);
		}

	    [Obsolete("Will be removed, do NOT use", true)]
        public ISchedulingResultStateHolder SchedulingStateHolder
        {
            get { return _stateHolder; }
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public ReadOnlyCollection<IScheduleDayPro> FullWeeksPeriodDays
        {
            get
            {
                IList<IScheduleDayPro> tempList = new List<IScheduleDayPro>(_fullWeeksPeriodDays.Values);
                return new ReadOnlyCollection<IScheduleDayPro>(tempList);
            }
        }

        public ReadOnlyCollection<IScheduleDayPro> OuterWeeksPeriodDays
        {
            get
            {
                IList<IScheduleDayPro> tempList = new List<IScheduleDayPro>(_outerWeeksPeriodDays.Values);
                return new ReadOnlyCollection<IScheduleDayPro>(tempList);
            }
        }

        public ReadOnlyCollection<IScheduleDayPro> WeekBeforeOuterPeriodDays
        {
            get 
            {
                IList<IScheduleDayPro> tempList = new List<IScheduleDayPro>(_weekBeforeOuterPeriodDays.Values);
                return new ReadOnlyCollection<IScheduleDayPro>(tempList);
            }
        }

        public ReadOnlyCollection<IScheduleDayPro> WeekAfterOuterPeriodDays
        {
            get
            {
                IList<IScheduleDayPro> tempList = new List<IScheduleDayPro>(_weekAfterOuterPeriodDays.Values);
                return new ReadOnlyCollection<IScheduleDayPro>(tempList);
            }
        }

        public IDictionary<DateOnly, IScheduleDayPro> WeekBeforeOuterPeriodDictionary
        {
            get { return _weekBeforeOuterPeriodDays; }
        }

        public IDictionary<DateOnly, IScheduleDayPro> WeekAfterOuterPeriodDictionary
        {
            get { return _weekAfterOuterPeriodDays; }
        }

        public IDictionary<DateOnly, IScheduleDayPro> OuterWeeksPeriodDictionary
        {
            get { return _outerWeeksPeriodDays; }
        }

        public IDictionary<DateOnly, IScheduleDayPro> FullWeeksPeriodDictionary
        {
            get { return _fullWeeksPeriodDays; }
        }

        public ReadOnlyCollection<IScheduleDayPro> EffectivePeriodDays
        {
            get
            {
                IList<IScheduleDayPro> tempList = new List<IScheduleDayPro>(_effectivePeriodDays.Values);
                return new ReadOnlyCollection<IScheduleDayPro>(tempList);
            }
        }

        public ReadOnlyCollection<IScheduleDayPro>  UnlockedDays
        {
            get
            {
                IList<IScheduleDayPro> tempList = new List<IScheduleDayPro>(_unLockedDays.Values);
                return new ReadOnlyCollection<IScheduleDayPro>(tempList);
            }
        }

		public void UnlockPeriod(DateOnlyPeriod period)
		{
			foreach (var dateOnly in period.DayCollection())
			{
				if (!_effectivePeriodDays.ContainsKey(dateOnly))
					throw new ArgumentOutOfRangeException("period");
				if (!_unLockedDays.ContainsKey(dateOnly))
					_unLockedDays.Add(dateOnly, _effectivePeriodDays[dateOnly]);
			}
		}

        public void LockPeriod(DateOnlyPeriod period)
        {
            foreach (var dateOnly in period.DayCollection())
            {
                if (_unLockedDays.ContainsKey(dateOnly))
                    _unLockedDays.Remove(dateOnly);
            }
        }

        public IScheduleDayPro GetScheduleDayByKey(DateOnly dateOnly)
        {
            IScheduleDayPro ret;
            _outerWeeksPeriodDays.TryGetValue(dateOnly, out ret);
            return ret;
        }

        public IVirtualSchedulePeriod SchedulePeriod
        {
            get { return _schedulePeriod; }
        }

        public IScheduleRange ActiveScheduleRange
        {
            get { return _activeScheduleRange; }
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