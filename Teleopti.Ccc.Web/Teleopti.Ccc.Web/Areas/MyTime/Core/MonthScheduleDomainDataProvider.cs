using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class MonthScheduleDomainDataProvider : IMonthScheduleDomainDataProvider
	{
		private readonly IScheduleProvider _scheduleProvider;

		public MonthScheduleDomainDataProvider(IScheduleProvider scheduleProvider)
		{
			_scheduleProvider = scheduleProvider;
		}

		public MonthScheduleDomainData Get(DateOnly date)
		{
			var firstDate = DateHelper.GetFirstDateInMonth(date.Date, CultureInfo.CurrentCulture);
			firstDate = DateHelper.GetFirstDateInWeek(firstDate, CultureInfo.CurrentCulture);
			var lastDate = DateHelper.GetLastDateInMonth(date.Date, CultureInfo.CurrentCulture);
			lastDate = DateHelper.GetLastDateInWeek(lastDate, CultureInfo.CurrentCulture);
			var period = new DateOnlyPeriod(new DateOnly(firstDate), new DateOnly(lastDate));
			var days = _scheduleProvider.GetScheduleForPeriod(period).Select(scheduleDay => new MonthScheduleDayDomainData
			{
				ScheduleDay = scheduleDay
			});

			return new MonthScheduleDomainData
			{
				CurrentDate = date,
				Days = days
			};
		}
	}
}