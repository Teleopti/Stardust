using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class OfficialWeekendDays : IOfficialWeekendDays
    {
        private readonly CultureInfo _culture;

        public OfficialWeekendDays(CultureInfo culture)
        {
            _culture = culture;
        }

        public DayOfWeek WeekStartDay
        {
            get { return _culture.DateTimeFormat.FirstDayOfWeek; }
        }

        public IList<DayOfWeek> WeekendDays()
        {
            switch(_culture.Name)
            {
                case "en-US":
                    return new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Saturday };
                case "ar-AE":
                    return new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Friday };
                default:
                    return new List<DayOfWeek>{ DayOfWeek.Saturday, DayOfWeek.Sunday };
            }
        }

        public IList<int> WeekendDayIndexes()
        {
            switch (_culture.Name)
            {
                case "en-US":
                case "ar-AE":
                    return new List<int> { 0, 6 };
                default:
                    return new List<int> { 5, 6 };
            }
        }

    }
}
