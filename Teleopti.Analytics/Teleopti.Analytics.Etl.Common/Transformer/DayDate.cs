using System;
using System.Globalization;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class DayDate
	{
		private DateTime _dateDate;
		private readonly int _dayInMonth;
		private readonly int _month;
		private readonly string _monthName;
		private readonly string _quarter;
		private readonly string _weekdayName;
		private readonly int _weekdayNumber;
		private readonly int _weekNumber;
		private readonly int _year;
		private readonly string _yearMonth;
		private readonly string _yearWeek;
		private readonly string _monthResourceKey;
		private readonly string _weekdayResourceKey;

		public DayDate(DateTime date, CultureInfo culture)
			: this()
		{
			_dateDate = date.Date;
			_year = date.Year;
			_month = date.Month;
			_yearMonth = GetYearMonth(_month);
			_monthName = DateHelper.GetMonthName(date, culture);
			_dayInMonth = date.Day;
			_weekdayNumber = GetDayOfWeek();
			_weekdayName = culture.DateTimeFormat.DayNames[(int)date.DayOfWeek];
			_weekNumber = DateHelper.WeekNumber(date, culture);
			_yearWeek = GetYearWeek(_weekNumber);
			_quarter = GetQuarterFromDate();
			_monthResourceKey = _month.GetMonthResourceKey();
			_weekdayResourceKey = _weekdayNumber.GetWeekDayResourceKey();
		}

		private DayDate()
		{
		}

		public DateTime DateDate
		{
			get { return _dateDate; }
		}

		public int DayInMonth
		{
			get { return _dayInMonth; }
		}

		public int Month
		{
			get { return _month; }
		}

		public string MonthName
		{
			get { return _monthName; }
		}

		public string MonthResourceKey
		{
			get { return _monthResourceKey; }
		}

		public string Quarter
		{
			get { return _quarter; }
		}

		public string WeekdayName
		{
			get { return _weekdayName; }
		}

		public string WeekdayResourceKey
		{
			get { return _weekdayResourceKey; }
		}

		public int WeekdayNumber
		{
			get { return _weekdayNumber; }
		}

		public int WeekNumber
		{
			get { return _weekNumber; }
		}

		public int Year
		{
			get { return _year; }
		}

		public string YearMonth
		{
			get { return _yearMonth; }
		}

		public string YearWeek
		{
			get { return _yearWeek; }
		}
		private int GetDayOfWeek()
		{
			//In data mart Sunday = 0 and Saturday = 6.
			int ret = (int)_dateDate.DayOfWeek;
			if (ret == 0) ret = 7;
			return ret;
		}

		private string GetQuarterFromDate() { return String.Concat(_year.ToString(CultureInfo.InvariantCulture), "Q", DateHelper.GetQuarter(_month)); }

		private string GetYearWeek(int week)
		{
			string datePart = week.ToString(CultureInfo.InvariantCulture);
			if (datePart.Length < 2)
				datePart = String.Concat("0", datePart);

			int year = _year;
			if (_dayInMonth <= 6 && _weekNumber > 51)
				year -= 1;
			if (_dayInMonth >= 26 && _weekNumber == 1)
				year += 1;

			return string.Format(CultureInfo.InvariantCulture, "{0}{1}", year, datePart);
		}

		private string GetYearMonth(int month)
		{
			string datePart = month.ToString(CultureInfo.InvariantCulture);
			if (datePart.Length < 2)
			{
				datePart = String.Concat("0", datePart);
			}
			return _year + datePart;
		}
	}
}