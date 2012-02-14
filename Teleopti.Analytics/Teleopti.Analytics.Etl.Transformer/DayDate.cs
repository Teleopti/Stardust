using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
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
            _monthResourceKey = GetMonthResourceKey();
            _weekdayResourceKey = GetWeekDayResourceKey();
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

        private string GetQuarterFromDate()
        {
            string quarter = "";
            switch (_month)
            {
                case 1:
                case 2:
                case 3:
                    quarter = "1";
                    break;
                case 4:
                case 5:
                case 6:
                    quarter = "2";
                    break;
                case 7:
                case 8:
                case 9:
                    quarter = "3";
                    break;
                case 10:
                case 11:
                case 12:
                    quarter = "4";
                    break;
            }

            return String.Concat(_year.ToString(CultureInfo.InvariantCulture), "Q", quarter);
        }

        private string GetYearWeek(int week)
        {
            string datePart = week.ToString(CultureInfo.InvariantCulture);
        	if (datePart.Length < 2)
        		datePart = String.Concat("0", datePart);

        	int year = _year;
			if (_dayInMonth == 1 && _weekNumber > 51)
				year -= 1;

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

        private string GetMonthResourceKey()
        {
            switch (_month)
            {
                case 1:
                    return "ResMonthJanuary";
                case 2:
                    return "ResMonthFebruary";
                case 3:
                    return "ResMonthMarch";
                case 4:
                    return "ResMonthApril";
                case 5:
                    return "ResMonthMay";
                case 6:
                    return "ResMonthJune";
                case 7:
                    return "ResMonthJuly";
                case 8:
                    return "ResMonthAugust";
                case 9:
                    return "ResMonthSeptember";
                case 10:
                    return "ResMonthOctober";
                case 11:
                    return "ResMonthNovember";
                case 12:
                    return "ResMonthDecember";
                default:
                    return "";
            }
        }

        private string GetWeekDayResourceKey()
        {
            switch (_weekdayNumber)
            {
                case 1:
                    return "ResDayOfWeekMonday";
                case 2:
                    return "ResDayOfWeekTuesday";
                case 3:
                    return "ResDayOfWeekWednesday";
                case 4:
                    return "ResDayOfWeekThursday";
                case 5:
                    return "ResDayOfWeekFriday";
                case 6:
                    return "ResDayOfWeekSaturday";
                case 7:
                    return "ResDayOfWeekSunday";
                default:
                    return "";
            }
        }
    }
}