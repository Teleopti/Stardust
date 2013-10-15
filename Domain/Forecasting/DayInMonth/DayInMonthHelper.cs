using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.DayInMonth
{
    public static class DayInMonthHelper
    {
        public static int DayIndex(DateOnly dateOnly)
        {
            double countOfDaysInMonth = DateTime.DaysInMonth(dateOnly.Year, dateOnly.Month);
            return Convert.ToInt32(dateOnly.Day/countOfDaysInMonth*30);
        }
    }
}