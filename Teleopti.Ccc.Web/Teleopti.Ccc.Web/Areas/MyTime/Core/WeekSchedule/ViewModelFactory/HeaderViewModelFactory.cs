using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public class HeaderViewModelFactory : IHeaderViewModelFactory
    {
        public HeaderViewModel CreateModel(IScheduleDay scheduleDay)
        {
            var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
            var day = CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(date.Date);
        	var dayDescription = createDayDescription(date, day);
            return new HeaderViewModel
                       {
                           Date = date.ToShortDateString(),
                           Title =
                               CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(
                                   date.Date.DayOfWeek),
                           DayDescription = dayDescription,
                           DayNumber = day.ToString(CultureInfo.CurrentUICulture)
                       };
        }

        private static string createDayDescription(DateOnly date, int dayOfMonth)
        {
            if (firstDayOfMonth(dayOfMonth) ||
                firstDayOfWeek(date))
            {
                return CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(
                    CultureInfo.CurrentCulture.Calendar.GetMonth(date.Date));
            }
            return string.Empty;
        }

        private static bool firstDayOfWeek(DateOnly date)
        {
        	return (date == DateHelper.GetFirstDateInWeek(date, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek));
        }

        private static bool firstDayOfMonth(int dayOfMonth)
        {
            return dayOfMonth == 1;
        }
    }
}