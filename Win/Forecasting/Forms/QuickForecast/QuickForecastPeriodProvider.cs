using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class QuickForecastPeriodProvider : IQuickForecastPeriodProvider
    {
        public DateOnlyPeriod DefaultStatisticPeriod
        {
            get
            {
                var yesterday = DateOnly.Today.AddDays(-1);
                var oneYearBack = CultureInfo.CurrentCulture.Calendar.AddYears(yesterday, -1);
                return new DateOnlyPeriod(new DateOnly(oneYearBack),yesterday);
            }
        }

        public DateOnlyPeriod DefaultTargetPeriod
        {
            get
            {
                var lastDayOfThisMonth = DateHelper.GetLastDateInMonth(DateOnly.Today, CultureInfo.CurrentCulture);
                var firstDayOfNextMonth = new DateOnly(lastDayOfThisMonth).AddDays(1);
                var firstDayOfFirstNotIncludedMonth = CultureInfo.CurrentCulture.Calendar.AddMonths(
                    firstDayOfNextMonth, 3);
                var lastDayOfLastMonth = new DateOnly(firstDayOfFirstNotIncludedMonth).AddDays(-1);
                return new DateOnlyPeriod(firstDayOfNextMonth, lastDayOfLastMonth);
            }
        }
    }
}