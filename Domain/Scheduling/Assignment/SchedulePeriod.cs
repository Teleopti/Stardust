using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	/// <summary>
	/// Class to hold scheduling periods
	/// </summary>
	/// <remarks>
	/// Created by: cs 
	/// Created date: 2008-03-10
	/// </remarks>
	public class SchedulePeriod : AggregateEntity, ISchedulePeriod
	{
		private DateOnly _dateFrom;
		private SchedulePeriodType _periodType;
		private int _number;
		private TimeSpan? _averageWorkTimePerDay;
		private int? _daysOff;
		private IList<IShiftCategoryLimitation> shiftCategoryLimitation = new List<IShiftCategoryLimitation>(); //rk: mappa som <set> när .net 4.0 börjar användas (då finns ISet<T> i .net)
		private int _mustHavePreference;
		private TimeSpan _balanceOut;
		private TimeSpan _extra;
		private Percent _seasonality;
		private TimeSpan _balanceIn;
		private TimeSpan? _periodTime;
		private readonly SchedulePeriodRangeCalculator _schedulePeriodRangeCalculator = new SchedulePeriodRangeCalculator();

		/// <summary>
		/// Default constructor
		/// </summary>
		protected SchedulePeriod()
		{
		}

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="creatorLocalDate">The creator local date.</param>
		/// <param name="type">The type.</param>
		/// <param name="number">The number.</param>
		public SchedulePeriod(DateOnly creatorLocalDate, SchedulePeriodType type, int number)
		{
			if (number < 1)
				throw new ArgumentOutOfRangeException("number", "Has to be > 0");

			_periodType = type;
			_dateFrom = creatorLocalDate;
			_number = number;
		}

		[RemoveMeWithToggle(Toggles.SchedulePeriod_HideChineseMonth_78424)]
		public virtual bool Toggle78424 { get; set; }

		/// <summary>
		/// gets or sets date from
		/// </summary>
		/// <remarks>
		/// Created by: cs 
		/// Created date: 2008-03-10
		/// </remarks>
		public virtual DateOnly DateFrom
		{
			get { return _dateFrom; }
			set { _dateFrom = value; }
		}


		/// <summary>
		/// gets type
		/// </summary>
		/// <remarks>
		/// Created by: cs 
		/// Created date: 2008-03-10
		/// </remarks>
		public virtual SchedulePeriodType PeriodType
		{
			get
			{
				if (Toggle78424 && _periodType == SchedulePeriodType.ChineseMonth)
					return SchedulePeriodType.Month;

				return _periodType;
			}
			set { _periodType = value; }
		}

		/// <summary>
		/// gets number
		/// </summary>
		/// <remarks>
		/// Created by: cs 
		/// Created date: 2008-03-10
		/// </remarks>
		public virtual int Number
		{
			get { return _number; }
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("value", "Has to be > 0");
				_number = value;
			}
		}

		/// <summary>
		/// Gets the average work time per day.
		/// </summary>
		/// <remarks>
		/// Created by: cs 
		/// Created date: 2008-03-10
		/// </remarks>
		public virtual TimeSpan AverageWorkTimePerDay
		{
			get
			{

				if (IsPeriodTimeOverride)
				{
					double periodTime = PeriodTime.Value.TotalMinutes;
					int totalWorkDay = workdaysForTotalPeriod();
					double periodMinutes = totalWorkDay == 0 ? 0 : periodTime/totalWorkDay;
					return TimeSpan.FromMinutes(periodMinutes);
				}

				if (IsAverageWorkTimePerDayOverride)
					return _averageWorkTimePerDay.Value;

				IPersonPeriod period = GetPersonPeriod();
				if (period == null) return TimeSpan.Zero;

				TimeSpan retTimeSpan = period.PersonContract.AverageWorkTimePerDay;
				return retTimeSpan;
			}
		}

		private int workdaysForTotalPeriod()
		{

			DateOnlyPeriod? totalPeriod = GetSchedulePeriod(_dateFrom);
			if (!totalPeriod.HasValue)
				return 0;

			DateOnly periodStart = totalPeriod.Value.StartDate;
			DateOnly periodEnd = totalPeriod.Value.EndDate;

			if (IsDaysOffOverride)
			{
				int ret = totalPeriod.Value.DayCount() - DaysOff.Value;
				return ret;
			}

			if (PeriodType == SchedulePeriodType.ChineseMonth)
			{
				int ret = totalPeriod.Value.DayCount() - getDaysOffForChineseMonth(_dateFrom);
				return ret;
			}

			IPersonPeriod period = GetPersonPeriod();
			if (period == null)
				return 0;
			int workDays = 0;
			DateOnly tempDate = periodStart;

			while (tempDate <= periodEnd)
			{
					if (period.PersonContract.ContractSchedule.IsWorkday(periodStart, tempDate, CurrentPerson.FirstDayOfWeek))
						workDays++;
				tempDate = tempDate.AddDays(1);
			}

			return workDays;
		}

		/// <summary>
		/// Gets the average work time per day for display.
		/// </summary>
		/// <remarks>
		/// Created by: cs 
		/// Created date: 2008-03-10
		/// </remarks>
		public virtual TimeSpan AverageWorkTimePerDayOverride
		{
			get
			{
				if (IsAverageWorkTimePerDayOverride)
					return _averageWorkTimePerDay.Value;

				return TimeSpan.MinValue;
			}
			set
			{
				_averageWorkTimePerDay = value;
			}
		}

		public virtual int? DaysOff
		{
			get { return _daysOff; }
			set { _daysOff = value; }
		}

		public virtual int GetDaysOff(DateOnly dateFrom)
		{
			if (_daysOff.HasValue) // if days off overriden
				return (int)(_daysOff.Value * notFullSchedulePeriodFactor(dateFrom));

			if (PeriodType == SchedulePeriodType.ChineseMonth)
				return (int)(getDaysOffForChineseMonth(dateFrom) * notFullSchedulePeriodFactor(dateFrom));

			DateOnlyPeriod? period = GetSchedulePeriod(dateFrom);
			if (!period.HasValue)
				return 0;

			DateOnly startDate = period.Value.StartDate;
			DateOnly endDate = period.Value.EndDate;
			int daysOff = 0;

			while (startDate <= endDate)
			{
				var personPeriod = CurrentPerson?.Period(startDate);
				if (personPeriod?.PersonContract?.ContractSchedule != null)
				{

					if (!personPeriod.PersonContract.ContractSchedule.IsWorkday(period.Value.StartDate, startDate,CurrentPerson.FirstDayOfWeek))
						daysOff++;
				}
				startDate = startDate.AddDays(1);
			}

			return daysOff;
		}

		private double notFullSchedulePeriodFactor(DateOnly dateFrom)
		{
			DateOnlyPeriod totalPeriod = _schedulePeriodRangeCalculator.PeriodForType(dateFrom,
				new SchedulePeriodForRangeCalculation
				{
					Culture = CurrentPerson.PermissionInformation.Culture(),
					Number = _number,
					PeriodType = _periodType,
					StartDate = dateFrom
				});
			int totalPeriodLength = totalPeriod.DayCount();
			int periodLength = contractPeriod(dateFrom);

			return periodLength/(double)totalPeriodLength;
		}

		private int contractPeriod(DateOnly dateFrom)
		{
			DateOnlyPeriod totalPeriod = _schedulePeriodRangeCalculator.PeriodForType(dateFrom,
				new SchedulePeriodForRangeCalculation
				{
					Culture = CurrentPerson.PermissionInformation.Culture(),
					Number = _number,
					PeriodType = _periodType,
					StartDate = dateFrom
				});

			DateOnly startDate = totalPeriod.StartDate;
			DateOnly endDate = totalPeriod.EndDate;

			int days = 0;
			while (startDate <= endDate)
			{
				IPersonPeriod personPeriod = CurrentPerson?.Period(startDate);
				if (personPeriod?.PersonContract?.ContractSchedule != null)
				{
					days++;
				}
				startDate = startDate.AddDays(1);
			}

			return days;
		}

		public virtual DateOnly RealDateTo()
		{
			if (PeriodType == SchedulePeriodType.Day)
				return (DateFrom.AddDays(Number - 1));

			if (PeriodType == SchedulePeriodType.Week)
				return (DateFrom.AddDays((Number * 7 - 1)));

			if (PeriodType == SchedulePeriodType.Month)
				return new DateOnly(DateFrom.Date.AddMonths(Number).AddDays(-1));

			return DateOnly.MinValue;
		}

		public virtual int GetWorkdays()
		{
			var period = _schedulePeriodRangeCalculator.PeriodForType(_dateFrom,new SchedulePeriodForRangeCalculation{Culture = CurrentPerson.PermissionInformation.Culture(),Number = _number,PeriodType = _periodType,StartDate = _dateFrom}); 
			return getWorkDaysForPeriod(period);
		}

		public virtual ReadOnlyCollection<IShiftCategoryLimitation> ShiftCategoryLimitationCollection()
		{
			return new ReadOnlyCollection<IShiftCategoryLimitation>(shiftCategoryLimitation);
		}

		public virtual bool IsPeriodTimeOverride
		{
			get { return _periodTime.HasValue; }
		}

		public virtual TimeSpan? PeriodTime
		{
			get { return _periodTime; }
			set { _periodTime = value; }
		}

		private IPerson CurrentPerson
		{
			get
			{
				return (IPerson)Parent;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is days off override.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is days off override; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-07-10
		/// </remarks>
		public virtual bool IsDaysOffOverride
		{
			get { return _daysOff.HasValue; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is average work time per day override.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is average work time per day override; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-07-10
		/// </remarks>
		public virtual bool IsAverageWorkTimePerDayOverride
		{
			get { return _averageWorkTimePerDay.HasValue; }
		}

		/// <summary>
		/// Gets the schedule period.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-23
		/// </remarks>
		private DateOnlyPeriod? getSchedulePeriod()
		{
			return GetSchedulePeriod(_dateFrom);
		}

		public virtual DateOnlyPeriod? GetSchedulePeriod(DateOnly dateValue)
		{
			if (dateValue < _dateFrom) return null;
			CurrentPerson.PermissionInformation.Culture();
			return
				checkAgainstTerminalDate(_schedulePeriodRangeCalculator.PeriodForType(dateValue,
					new SchedulePeriodForRangeCalculation
					{
						Culture = CurrentPerson.PermissionInformation.Culture(),
						Number = _number,
						PeriodType = _periodType,
						StartDate = _dateFrom
					}));
		}

		/// <summary>
		/// Adjust for terminal date
		/// </summary>
		/// <param name="period"></param>
		/// <returns></returns>
		private DateOnlyPeriod? checkAgainstTerminalDate(DateOnlyPeriod period)
		{
			IPerson person = CurrentPerson;
			if (person == null)
				return null;

			if (person.TerminalDate.HasValue)
			{
				DateOnly terminalDate = person.TerminalDate.Value;

				if (terminalDate < period.StartDate) return null;

				if (period.Contains(terminalDate))
				{
					return new DateOnlyPeriod(period.StartDate, terminalDate);
				}
			}

			return period;
		}

		/// <summary>
		/// Gets the person periods.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-23
		/// </remarks>
		public virtual IPersonPeriod GetPersonPeriod()
		{
			DateOnlyPeriod? datePeriod = getSchedulePeriod();

			return datePeriod.HasValue ? CurrentPerson.Period(datePeriod.Value.StartDate) : null;
		}

		/// <summary>
		/// Resets the average work time per day.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-23
		/// </remarks>
		public virtual void ResetAverageWorkTimePerDay()
		{
			_averageWorkTimePerDay = null;
		}

		/// <summary>
		/// Resets the period time.
		/// </summary>
		/// <remarks>
		/// Created by: tamasb
		/// Created date: 2012-06-21
		/// </remarks>
		public virtual void ResetPeriodTime()
		{
			_periodTime = null;
		}

		/// <summary>
		/// Sets the days off.
		/// </summary>
		/// <param name="value">The number of days.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-07-10
		/// </remarks>
		public virtual void SetDaysOff(int value)
		{
			InParameter.ValueMustBePositive(nameof(DaysOff), value);
			if (value > 999)
				throw new ArgumentOutOfRangeException(nameof(DaysOff), "The number of days off cannot exceed 999.");

			_daysOff = value;
		}

		/// <summary>
		/// Resets the days off.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-07-10
		/// </remarks>
		public virtual void ResetDaysOff()
		{
			_daysOff = null;
		}

		public virtual void AddShiftCategoryLimitation(IShiftCategoryLimitation shiftCategoryLimitationToAdd)
		{
			var lim = findLimitationByCategory(shiftCategoryLimitationToAdd.ShiftCategory);
			if (lim != null)
				throw new ArgumentException("A ShiftCategoryLimitation with shift category " + shiftCategoryLimitationToAdd.ShiftCategory.Id + " is already present");

			shiftCategoryLimitation.Add(shiftCategoryLimitationToAdd);
		}

		public virtual void RemoveShiftCategoryLimitation(IShiftCategory shiftCategory)
		{
			var lim = findLimitationByCategory(shiftCategory);
			if (lim != null)
				shiftCategoryLimitation.Remove(lim);
		}

		private IShiftCategoryLimitation findLimitationByCategory(IShiftCategory shiftCategory)
		{
			foreach (var limitation in shiftCategoryLimitation)
			{
				if (limitation.ShiftCategory.Equals(shiftCategory))
					return limitation;
			}
			return null;
		}

		private int getWorkDaysForPeriod(DateOnlyPeriod period)
		{
			if (CurrentPerson == null) return 0;

			if (PeriodType == SchedulePeriodType.ChineseMonth)
			{
				int daysOff = getDaysOffForChineseMonth(period.StartDate);
				return period.DayCount() - daysOff;
			}

			if (_daysOff.HasValue) // if days off are overriden
			{
				return period.DayCount() - _daysOff.Value;
			}

			var dayDetails = CurrentPerson.AverageWorkTimes(period);
			return dayDetails.Count(d => d.IsWorkDay);
		}

		private int getDaysOffForChineseMonth(DateOnly startDate)
		{
			DateTime lastDateInMonth = DateHelper.GetLastDateInMonth(startDate.Date, CurrentPerson.PermissionInformation.Culture());
			int daysInMonth = lastDateInMonth.Day;

			if (daysInMonth == 31)
				return 8;

			return 7;
		}

		public virtual int MustHavePreference
		{
			get { return _mustHavePreference; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("MustHavePreference" + _mustHavePreference,
														  "MustHavePreference must be zero ore more");
				_mustHavePreference = value;
			}
		}

		public virtual TimeSpan BalanceIn
		{
			get { return _balanceIn; }
			set
			{
				var roundedValue = TimeSpan.FromMinutes(Math.Round(value.TotalMinutes, 0));
				_balanceIn = roundedValue;
			}
		}

		public virtual TimeSpan Extra
		{
			get { return _extra; }
			set
			{
				var roundedValue = TimeSpan.FromMinutes(Math.Round(value.TotalMinutes, 0));
				_extra = roundedValue;
			}
		}

		public virtual Percent Seasonality
		{
			get { return _seasonality; }
			set { _seasonality = value; }
		}

		public virtual TimeSpan BalanceOut
		{
			get { return _balanceOut; }
			set
			{
				var roundedValue = TimeSpan.FromMinutes(Math.Round(value.TotalMinutes, 0));
				_balanceOut = roundedValue;
			}
		}

		#region ICloneable Members

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-07-31
		/// </remarks>
		public virtual object Clone()
		{
			SchedulePeriod retobj = (SchedulePeriod)MemberwiseClone();
			retobj.shiftCategoryLimitation = new List<IShiftCategoryLimitation>();
			foreach (var limitation in shiftCategoryLimitation)
			{
				retobj.AddShiftCategoryLimitation((IShiftCategoryLimitation)limitation.Clone());
			}
			retobj.SetId(null);

			return retobj;
		}

		#endregion
	}

	public class PeriodIncrementorFactory
	{
		public IIncreasePeriodByOne PeriodIncrementor(SchedulePeriodType theType, CultureInfo cultureInfo)
		{
			switch (theType)
			{
				case SchedulePeriodType.Week:
					return new IncreaseWeekByOne();
				case SchedulePeriodType.Month:
				case SchedulePeriodType.ChineseMonth:
					return new IncreaseMonthByOne(cultureInfo);
				default: //Day
					return new IncreaseDayByOne();
			}
		}
	}

	public class IncreaseDayByOne : IIncreasePeriodByOne
	{
		public DateOnly Increase(DateOnly currentStartDate, int number)
		{
			return currentStartDate.AddDays(number - 1);
		}

		public DateOnly EvaluateProperInitialStartDate(DateOnly currentStartDate, int number, DateOnly requestedDate)
		{
			var difference = (int)requestedDate.Date.Subtract(currentStartDate.Date).TotalDays;
			var wholeVirtualPeriods = difference / number;
			return Increase(currentStartDate, number * wholeVirtualPeriods).AddDays(1);
		}
	}

	public class IncreaseMonthByOne : IIncreasePeriodByOne
	{
		private readonly CultureInfo _cultureInfo;

		public IncreaseMonthByOne(CultureInfo cultureInfo)
		{
			_cultureInfo = cultureInfo ?? CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.LCID);
		}

		public DateOnly Increase(DateOnly currentStartDate, int number)
		{
			return new DateOnly(_cultureInfo.Calendar.AddMonths(currentStartDate.Date, number)).AddDays(-1);
		}

		public DateOnly EvaluateProperInitialStartDate(DateOnly currentStartDate, int number, DateOnly requestedDate)
		{
			var difference = (int)requestedDate.Date.Subtract(currentStartDate.Date).TotalDays;
			var wholeVirtualPeriods = difference / (32 * number);
			return Increase(currentStartDate, number * wholeVirtualPeriods).AddDays(1);
		}
	}

	public class IncreaseWeekByOne : IIncreasePeriodByOne
	{
		public DateOnly Increase(DateOnly currentStartDate, int number)
		{
			return currentStartDate.AddDays((number * 7) - 1);
		}

		public DateOnly EvaluateProperInitialStartDate(DateOnly currentStartDate, int number, DateOnly requestedDate)
		{
			var difference = (int)requestedDate.Date.Subtract(currentStartDate.Date).TotalDays;
			var wholeVirtualPeriods = difference / (7 * number);
			return Increase(currentStartDate, number * wholeVirtualPeriods).AddDays(1);
		}
	}

	public class SchedulePeriodRangeCalculator
	{
		private readonly PeriodIncrementorFactory _periodIncrementorFactory = new PeriodIncrementorFactory();

		public DateOnlyPeriod PeriodForType(DateOnly requestedDate, SchedulePeriodForRangeCalculation period)
		{
			var periodIncrementor = _periodIncrementorFactory.PeriodIncrementor(period.PeriodType, period.Culture);
			var start = periodIncrementor.EvaluateProperInitialStartDate(period.StartDate, period.Number, requestedDate);
			var currentPeriod = new DateOnlyPeriod(start, periodIncrementor.Increase(start, period.Number));
			while (!currentPeriod.Contains(requestedDate))
			{
				start = periodIncrementor.Increase(start, period.Number).AddDays(1);
				currentPeriod = new DateOnlyPeriod(start, periodIncrementor.Increase(start, period.Number));
			}

			return currentPeriod;
		}
	}
}
