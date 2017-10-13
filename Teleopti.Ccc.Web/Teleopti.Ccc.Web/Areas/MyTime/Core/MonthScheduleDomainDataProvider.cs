using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class MonthScheduleDomainDataProvider : IMonthScheduleDomainDataProvider
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IToggleManager _toggleManager;
		private readonly ISeatOccupancyProvider _seatBookingProvider;

		public MonthScheduleDomainDataProvider(IScheduleProvider scheduleProvider, IToggleManager toggleManager, ISeatOccupancyProvider seatBookingProvider)
		{
			_scheduleProvider = scheduleProvider;
			_toggleManager = toggleManager;
			_seatBookingProvider = seatBookingProvider;
		}

		public MonthScheduleDomainData Get(DateOnly date, bool loadSeatBooking)
		{

			var firstDate = DateHelper.GetFirstDateInMonth(date.Date, CultureInfo.CurrentCulture);
			firstDate = DateHelper.GetFirstDateInWeek(firstDate, CultureInfo.CurrentCulture);
			var lastDate = DateHelper.GetLastDateInMonth(date.Date, CultureInfo.CurrentCulture);
			lastDate = DateHelper.GetLastDateInWeek(lastDate, CultureInfo.CurrentCulture);
			var period = new DateOnlyPeriod(new DateOnly(firstDate), new DateOnly(lastDate));

			var scheduleDays =
				_scheduleProvider.GetScheduleForPeriod(period, new Domain.Common.ScheduleDictionaryLoadOptions(false, false))
					.ToList();
			var showSeatBookings = _toggleManager.IsEnabled(Toggles.MyTimeWeb_ShowSeatBookingMonthView_39068);
			var seatBookings = loadSeatBooking && showSeatBookings ? _seatBookingProvider.GetSeatBookingsForScheduleDays(scheduleDays) : null;

			var days = scheduleDays.Select(scheduleDay => new MonthScheduleDayDomainData
			{
				ScheduleDay = scheduleDay,
				SeatBookingInformation = seatBookings?.Where(seatBooking => seatBooking.BelongsToDate == scheduleDay.DateOnlyAsPeriod.DateOnly).ToArray()
			});

			return new MonthScheduleDomainData
			{
				CurrentDate = date,
				Days = days
			};
		}
	}
}