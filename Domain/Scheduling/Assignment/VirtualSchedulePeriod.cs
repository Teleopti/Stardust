using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class VirtualSchedulePeriod : IVirtualSchedulePeriod
    {
        private readonly SchedulePeriod _schedulePeriod;
        private readonly IPerson _person;
		private readonly IContract _contract;
		private readonly IContractSchedule _contractSchedule;
		private readonly IPartTimePercentage _partTimePercentage;
		private readonly TimeSpan _averageWorkTimePerDay;

        private DateOnlyPeriod _thePeriodWithTheDateIn;

        private readonly TimeSpan _minTimeSchedulePeriod = TimeSpan.Zero;
		private static readonly PeriodIncrementorFactory _periodIncrementorFactory = new PeriodIncrementorFactory();
	    
	    public VirtualSchedulePeriod(IPerson person, DateOnly dateOnly, IVirtualSchedulePeriodSplitChecker splitChecker) : this(person,dateOnly,person.Period(dateOnly),person.SchedulePeriod(dateOnly),splitChecker)
        {
        }

	    public VirtualSchedulePeriod(IPerson person, DateOnly date, IPersonPeriod personPeriod, ISchedulePeriod schedulePeriod, IVirtualSchedulePeriodSplitChecker splitChecker)
	    {
			if (person == null) throw new ArgumentNullException(nameof(person));
			if (splitChecker == null) throw new ArgumentNullException(nameof(splitChecker));

			_schedulePeriod = (SchedulePeriod) schedulePeriod;
			_person = person;

			if (_schedulePeriod != null)
			{
				Number = _schedulePeriod.Number;
				PeriodType = _schedulePeriod.PeriodType;

				//just so we get a period we do this before we check PersonPeriod (maybe not correct but used in some tests)
				_thePeriodWithTheDateIn = getRolledDateOnlyPeriodForSchedulePeriod(date, _schedulePeriod, person.PermissionInformation.Culture());

				var virtualPeriod = splitChecker.Check(_thePeriodWithTheDateIn, date);
				if (!virtualPeriod.HasValue) return;

				if (personPeriod != null && personPeriod.PersonContract != null)
				{
					_contract = personPeriod.PersonContract.Contract;
					_contractSchedule = personPeriod.PersonContract.ContractSchedule;
					_partTimePercentage = personPeriod.PersonContract.PartTimePercentage;
					_averageWorkTimePerDay = personPeriod.PersonContract.AverageWorkTimePerDay;
					_minTimeSchedulePeriod = _contract.MinTimeSchedulePeriod;
				}

				if (!virtualPeriod.Value.Equals(_thePeriodWithTheDateIn))
				{
					_thePeriodWithTheDateIn = virtualPeriod.Value;
					PeriodType = SchedulePeriodType.Day;
					Number = _thePeriodWithTheDateIn.DayCount();
				}
			}
	    }

	    private DateOnlyPeriod getRolledDateOnlyPeriodForSchedulePeriod(DateOnly requestedDateTime, ISchedulePeriod thePeriod, CultureInfo cultureInfo)
        {
            var periodIncrementor = _periodIncrementorFactory.PeriodIncrementor(thePeriod.PeriodType, cultureInfo);
			var start = periodIncrementor.EvaluateProperInitialStartDate(thePeriod.DateFrom, Number, requestedDateTime);
			
            var currentPeriod = new DateOnlyPeriod(start, periodIncrementor.Increase(start, Number));
            while (!currentPeriod.Contains(requestedDateTime))
            {
                start = periodIncrementor.Increase(start, Number).AddDays(1);
                currentPeriod = new DateOnlyPeriod(start, periodIncrementor.Increase(start, Number));
            }

            return currentPeriod;
        }

        public DateOnlyPeriod DateOnlyPeriod => _thePeriodWithTheDateIn;

	    public IPerson Person => _person;

	    public ReadOnlyCollection<IShiftCategoryLimitation> ShiftCategoryLimitationCollection()
        {
            if (_schedulePeriod == null)
                return new ReadOnlyCollection<IShiftCategoryLimitation>(new List<IShiftCategoryLimitation>());
            return _schedulePeriod.ShiftCategoryLimitationCollection();
        }

        public int MustHavePreference => _schedulePeriod.MustHavePreference;

	    public TimeSpan MinTimeSchedulePeriod => _minTimeSchedulePeriod;

	    public IContract Contract => _contract;

	    public IContractSchedule ContractSchedule => _contractSchedule;

	    public IPartTimePercentage PartTimePercentage => _partTimePercentage;

	    public bool IsValid => _schedulePeriod != null && _contract != null;

	    public SchedulePeriodType PeriodType { get; }

        public int Number { get; }

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
		        return _averageWorkTimePerDay;
		    }
		}

	    private int schedulePeriodWorkdays()
	    {
		    DateOnlyPeriod? totalPeriod = SchedulePeriodPeriod();
		    if (!totalPeriod.HasValue)
			    return 1;

		    DateOnly startDate = totalPeriod.Value.StartDate;
		    if (_schedulePeriod.IsDaysOffOverride || _schedulePeriod.PeriodType == SchedulePeriodType.ChineseMonth)
		    {
			    return SchedulePeriodPeriod().Value.DayCount() - _schedulePeriod.GetDaysOff(startDate);
		    }

		    return _person.AverageWorkTimes(totalPeriod.Value).Count(w => w.IsWorkDay);
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

            if (_contractSchedule == null)
                return 1;

			DateOnly? periodStart = Person.SchedulePeriodStartDate(_thePeriodWithTheDateIn.StartDate);
			if (!periodStart.HasValue)
				return 1;

			if (_schedulePeriod.IsDaysOffOverride || _schedulePeriod.PeriodType == SchedulePeriodType.ChineseMonth)
            {
				return _thePeriodWithTheDateIn.DayCount() - DaysOff();
            }

	        return _person.AverageWorkTimes(_thePeriodWithTheDateIn).Count(w => w.IsWorkDay);
        }

		private DateOnlyPeriod? SchedulePeriodPeriod()
		{
			if (!IsValid)
				return null;

			if (_contractSchedule == null)
				return null;

			return _schedulePeriod.GetSchedulePeriod(_thePeriodWithTheDateIn.StartDate);
		}

    	public int DaysOff()
        {
			if (!IsValid) return 0;

			DateOnlyPeriod? totalPeriod = SchedulePeriodPeriod();
			if (!totalPeriod.HasValue)
				return 0;

			if (_schedulePeriod.IsDaysOffOverride || _schedulePeriod.PeriodType == SchedulePeriodType.ChineseMonth)
			{
				int totalDaysOff = _schedulePeriod.GetDaysOff(_thePeriodWithTheDateIn.StartDate);
				double rawResult = Math.Round(totalDaysOff*notFullSchedulePeriodFactor(), 0);
				return (int)rawResult;
			}

			if (_contractSchedule == null)
				return 0;

			DateOnly? periodStart = Person.SchedulePeriodStartDate(_thePeriodWithTheDateIn.StartDate);
			if (!periodStart.HasValue)
				return 0;

    		return _person.AverageWorkTimes(_thePeriodWithTheDateIn).Count(w => !w.IsWorkDay);
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

	    private double percentOfOrginalPeriod()
	    {
			if (_contractSchedule == null || !IsValid)
				return 0;

			var originalPeriod = GetOriginalStartPeriodForType(_schedulePeriod, _person.PermissionInformation.Culture());
		    var intersection = DateOnlyPeriod.Intersection(originalPeriod);
		    if (intersection == null)
			    return 0;

			var originalWorkDays = (double)_schedulePeriod.GetWorkdays();
		    if (originalWorkDays == 0)
			    return 0;
		    var currentWorkDays = (double)Workdays();
		    return Math.Round(currentWorkDays / originalWorkDays, 2);
		}

		public virtual TimeSpan BalanceIn => TimeSpan.FromMinutes(_schedulePeriod?.BalanceIn.TotalMinutes ?? 0 * percentOfOrginalPeriod());

		public virtual TimeSpan Extra => TimeSpan.FromMinutes(_schedulePeriod?.Extra.TotalMinutes ?? 0 * percentOfOrginalPeriod());

		public virtual Percent Seasonality => _schedulePeriod.Seasonality;

		public virtual TimeSpan BalanceOut => TimeSpan.FromMinutes(_schedulePeriod?.BalanceOut.TotalMinutes ?? 0 * percentOfOrginalPeriod());

		public override bool Equals(object obj)
        {
            var casted = obj as VirtualSchedulePeriod;
            if (obj == null || casted == null)
            {
                return false;
            }

            return casted._person.Equals(_person) && _thePeriodWithTheDateIn == casted._thePeriodWithTheDateIn;
        }

        public override int GetHashCode()
        {
            return _person.GetHashCode() ^ _thePeriodWithTheDateIn.GetHashCode();
        }

        public bool IsOriginalPeriod()
        {
            var originalPeriod = GetOriginalStartPeriodForType(_schedulePeriod, _person.PermissionInformation.Culture());
            var intersection = DateOnlyPeriod.Intersection(originalPeriod);

	        return originalPeriod.DayCount() == intersection?.DayCount();
        }
    }
}
