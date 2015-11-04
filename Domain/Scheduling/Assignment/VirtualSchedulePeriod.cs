using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class VirtualSchedulePeriod : IVirtualSchedulePeriod
    {
        // the underlying SchedulePeriod
        private readonly SchedulePeriod _schedulePeriod;
        private readonly DateOnly _requestedDateOnly;
        private readonly IPerson _person;
        // we can use this to check if we must change the type from for example Month to day
        private DateOnlyPeriod _thePeriodWithTheDateIn;
        private readonly IPersonContract _personContract;

        private int _number;
        private readonly TimeSpan _minTimeSchedulePeriod = TimeSpan.Zero;
		private static readonly PeriodIncrementorFactory _periodIncrementorFactory = new PeriodIncrementorFactory();

        public VirtualSchedulePeriod(IPerson person, DateOnly dateOnly, IVirtualSchedulePeriodSplitChecker splitChecker)
        {
            if (person == null)
                throw new ArgumentNullException("person");

            if (splitChecker == null)
                throw new ArgumentNullException("splitChecker");

            _schedulePeriod = (SchedulePeriod)schedulePeriodOnDate(person, dateOnly);

            _person = person;
            _requestedDateOnly = dateOnly;

            if (_schedulePeriod != null)
            {
                _number = _schedulePeriod.Number;
                PeriodType = _schedulePeriod.PeriodType;

                //just so we get a period we do this before we check PersonPeriod (maybe not correct but used in some tests)
                _thePeriodWithTheDateIn = getRolledDateOnlyPeriodForSchedulePeriod(_requestedDateOnly, _schedulePeriod, person.PermissionInformation.Culture());

                var virtualPeriod = splitChecker.Check(_thePeriodWithTheDateIn, dateOnly);

                if (!virtualPeriod.HasValue)
                    return;

                IPersonPeriod personPeriod = person.Period(virtualPeriod.Value.StartDate);

                _personContract = personPeriod.PersonContract;

                if (_personContract != null && _personContract.Contract != null)
                    _minTimeSchedulePeriod = _personContract.Contract.MinTimeSchedulePeriod;

                if (!virtualPeriod.Value.Equals(_thePeriodWithTheDateIn))
                {
                    _thePeriodWithTheDateIn = virtualPeriod.Value;
                    PeriodType = SchedulePeriodType.Day;
                    Number = (_thePeriodWithTheDateIn.EndDate.Date - _thePeriodWithTheDateIn.StartDate.Date).Days + 1;
                }
            }
        }

        private static ISchedulePeriod schedulePeriodOnDate(IPerson person, DateOnly dateOnly)
        {
            ISchedulePeriod period = null;

            TimeSpan minVal = TimeSpan.MaxValue;

            //get list with periods where startdate is less than inparam date
            IList<ISchedulePeriod> periods = person.PersonSchedulePeriodCollection.Where(s => s.DateFrom <= dateOnly
                || (s.DateFrom.Date == dateOnly.Date)).ToList();

            //find period
            foreach (ISchedulePeriod p in periods)
            {
                if ((p.DateFrom.Date == dateOnly.Date))
                {
                    // Latest period is startdate equal to given date.
                    return p;
                }
                //get diff between inpara and startdate
                TimeSpan diff = dateOnly.Subtract(p.DateFrom);

                //check against smallest diff and check that inparam is greater than startdate
                if (diff < minVal && diff.TotalMinutes >= 0)
                {
                    minVal = diff;
                    period = p;
                }
            }

            return period;
        }

        private DateOnlyPeriod getRolledDateOnlyPeriodForSchedulePeriod(DateOnly requestedDateTime, ISchedulePeriod thePeriod, CultureInfo cultureInfo)
        {
            var periodIncrementor = _periodIncrementorFactory.PeriodIncrementor(thePeriod.PeriodType, cultureInfo);
			DateOnly start = periodIncrementor.EvaluateProperInitialStartDate(thePeriod.DateFrom, _number, requestedDateTime);
            var currentPeriod = new DateOnlyPeriod(start, periodIncrementor.Increase(start, _number));
            while (!currentPeriod.Contains(requestedDateTime))
            {
                start = periodIncrementor.Increase(start, _number).AddDays(1);
                currentPeriod = new DateOnlyPeriod(start, periodIncrementor.Increase(start, _number));
            }

            return currentPeriod;
        }

        public DateOnlyPeriod DateOnlyPeriod
        {
            get
            {
                return _thePeriodWithTheDateIn;
            }
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public ReadOnlyCollection<IShiftCategoryLimitation> ShiftCategoryLimitationCollection()
        {
            if (_schedulePeriod == null)
                return new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation>());
            return _schedulePeriod.ShiftCategoryLimitationCollection();
        }

        public int MustHavePreference
        {
            get { return _schedulePeriod.MustHavePreference; }
        }

        public TimeSpan MinTimeSchedulePeriod
        {
            get { return _minTimeSchedulePeriod; }
        }

        public IContract Contract
        {
            get { return _personContract.Contract; }
        }

        public IContractSchedule ContractSchedule
        {
            get { return _personContract.ContractSchedule; }
        }

        public IPartTimePercentage PartTimePercentage
        {
            get { return _personContract.PartTimePercentage; }
        }

        public bool IsValid
        {
            get { return _schedulePeriod != null && _personContract != null; }
        }

        public SchedulePeriodType PeriodType { get; private set; }

        public int Number
        {
            get { return _number; }
            private set { _number = value; }
        }

		public TimeSpan AverageWorkTimePerDay
		{
		    get
		    {
		        if (!IsValid)
		            return TimeSpan.Zero;
		        if (_schedulePeriod.IsPeriodTimeOverride)
		        {
		            double periodTime = _schedulePeriod.PeriodTime.Value.TotalMinutes;
		                //PeriodTime will NOT be null, as we check for PeriodTimeOverride
		            int schedulePeriodWorkdays = this.schedulePeriodWorkdays();
		            double periodMinutes = 0;
		            if (schedulePeriodWorkdays > 0)
		            {
		                periodMinutes = periodTime/schedulePeriodWorkdays;
		            }
		            return TimeSpan.FromMinutes(periodMinutes);
		        }
		        if (_schedulePeriod.IsAverageWorkTimePerDayOverride)
		            return _schedulePeriod.AverageWorkTimePerDay;
		        return _personContract.AverageWorkTimePerDay;
		    }
		}

		public TimeSpan PeriodTarget()
		{
			if (!IsValid)
                return TimeSpan.Zero;

			int workDays = Workdays();
			double minutes = Math.Round(AverageWorkTimePerDay.TotalMinutes * workDays, 0);
			return TimeSpan.FromMinutes(minutes);
		}

        public int Workdays()
        {
            if (!IsValid)
                return 1;

            if (_personContract == null || _personContract.ContractSchedule == null)
                return 1;

            DateOnly startDate = _thePeriodWithTheDateIn.StartDate;
            DateOnly endDate = _thePeriodWithTheDateIn.EndDate;

			DateOnly? periodStart = Person.SchedulePeriodStartDate(_thePeriodWithTheDateIn.StartDate);
			if (!periodStart.HasValue)
				return 1;

            int workDays = 0;

			if (_schedulePeriod.IsDaysOffOverride || _schedulePeriod.PeriodType == SchedulePeriodType.ChineseMonth)
            {
				return _thePeriodWithTheDateIn.DayCount() - DaysOff();
            }

            while (startDate <= endDate)
            {
                if (_person != null)
                {
					if (_personContract.ContractSchedule.IsWorkday(periodStart.Value, startDate))                                                   
                        workDays++;
                }
                startDate = startDate.AddDays(1);
            }

            return workDays;
        }

        private int schedulePeriodWorkdays()
		{
			DateOnlyPeriod? totalPeriod = SchedulePeriodPeriod();
			if (!totalPeriod.HasValue)
				return 1;

			DateOnly startDate = totalPeriod.Value.StartDate;
			DateOnly tempDate = totalPeriod.Value.StartDate;
			DateOnly endDate = totalPeriod.Value.EndDate;

			if (_schedulePeriod.IsDaysOffOverride || _schedulePeriod.PeriodType == SchedulePeriodType.ChineseMonth)
			{
				return SchedulePeriodPeriod().Value.DayCount() - _schedulePeriod.GetDaysOff(startDate);
			}

			int workDays = 0;
			while (tempDate <= endDate)
			{
				if (_person != null)
				{
					if (_personContract.ContractSchedule.IsWorkday(startDate, tempDate))
						workDays++;
				}
				tempDate = tempDate.AddDays(1);
			}

			return workDays;
		}

		private DateOnlyPeriod? SchedulePeriodPeriod()
		{
			if (!IsValid)
				return null;

			if (_personContract.ContractSchedule == null)
				return null;

			return _schedulePeriod.GetSchedulePeriod(_thePeriodWithTheDateIn.StartDate);
		}


    	public int DaysOff()
        {
			if (!IsValid)
			    return 0;

			DateOnlyPeriod? totalPeriod = SchedulePeriodPeriod();
			if (!totalPeriod.HasValue)
				return 0;

			if (_schedulePeriod.IsDaysOffOverride || _schedulePeriod.PeriodType == SchedulePeriodType.ChineseMonth)
			{
				int totalDaysOff = _schedulePeriod.GetDaysOff(_thePeriodWithTheDateIn.StartDate);
				double rawResult = Math.Round(totalDaysOff*notFullSchedulePeriodFactor(), 0);
				return (int)rawResult;
			}

			if (_personContract.ContractSchedule == null)
				return 0;

			DateOnly? periodStart = Person.SchedulePeriodStartDate(_thePeriodWithTheDateIn.StartDate);
			if (!periodStart.HasValue)
				return 0;

			DateOnly startDate = _thePeriodWithTheDateIn.StartDate;
			DateOnly endDate = _thePeriodWithTheDateIn.EndDate;
			int daysOff = 0;
			while (startDate <= endDate)
			{
				if (!_personContract.ContractSchedule.IsWorkday(periodStart.Value, startDate))
					daysOff++;

				startDate = startDate.AddDays(1);
			}

			return daysOff;
        }

		private double notFullSchedulePeriodFactor()
		{
			DateOnlyPeriod? totalPeriod = SchedulePeriodPeriod();
			int totalPeriodLength = totalPeriod.Value.DayCount();
			int periodLength = _thePeriodWithTheDateIn.DayCount();

			return (double)periodLength/(double)totalPeriodLength;
		}

        public static DateOnlyPeriod GetOriginalStartPeriodForType(ISchedulePeriod schedulePeriod, CultureInfo cultureInfo)
        {
            DateOnly start = schedulePeriod.DateFrom;
            var currentPeriod = new DateOnlyPeriod(start, _periodIncrementorFactory.PeriodIncrementor(schedulePeriod.PeriodType, cultureInfo).Increase(start, schedulePeriod.Number));
            return currentPeriod;
        }

        public Percent GetPercentageWorkdaysOfOriginalPeriod(DateOnlyPeriod originalPeriod, ISchedulePeriod schedulePeriod)
        {
            var intersection = DateOnlyPeriod.Intersection(originalPeriod);
            if (intersection == null)
                return new Percent(0);

            var originalWorkDays = (double)((SchedulePeriod)schedulePeriod).GetWorkdays();
            if (originalWorkDays == 0)
                return new Percent(0);
            var currentWorkDays = (double)Workdays();

            return new Percent(Math.Round((currentWorkDays / originalWorkDays), 2));
        }

        private TimeSpan percentOfOriginalPeriod(TimeSpan originaTimeSpan)
        {
            if (!IsValid)
                return TimeSpan.Zero;

            if (_personContract == null || _personContract.ContractSchedule == null)
                return TimeSpan.Zero;

            var originalPeriod = GetOriginalStartPeriodForType(_schedulePeriod, _person.PermissionInformation.Culture());
            var percent = GetPercentageWorkdaysOfOriginalPeriod(originalPeriod, _schedulePeriod);
            return TimeSpan.FromMinutes(originaTimeSpan.TotalMinutes * percent.Value);
        }

        public virtual TimeSpan BalanceIn
        {
            get { return percentOfOriginalPeriod(_schedulePeriod.BalanceIn); }
        }

        public virtual TimeSpan Extra
        {
            get { return percentOfOriginalPeriod(_schedulePeriod.Extra); }
        }

        public virtual Percent Seasonality
        {
            get
            {
                return _schedulePeriod.Seasonality;
            }
        }

        public virtual TimeSpan BalanceOut
        {
            get { return percentOfOriginalPeriod(_schedulePeriod.BalanceOut); }
        }

        public override bool Equals(object obj)
        {
            var casted = obj as VirtualSchedulePeriod;
            if (obj == null || casted == null)
            {
                return false;
            }

            return (casted._person.Equals(_person) && _thePeriodWithTheDateIn == casted._thePeriodWithTheDateIn);
        }

        public override int GetHashCode()
        {
            return (_person.GetHashCode() ^ _thePeriodWithTheDateIn.GetHashCode());
        }

        public bool IsOriginalPeriod()
        {
            var originalPeriod = GetOriginalStartPeriodForType(_schedulePeriod, _person.PermissionInformation.Culture());
            var intersection = DateOnlyPeriod.Intersection(originalPeriod);
            if (!intersection.HasValue)
                return false;

            return originalPeriod.DayCount() == intersection.Value.DayCount();
        }
    }
}
