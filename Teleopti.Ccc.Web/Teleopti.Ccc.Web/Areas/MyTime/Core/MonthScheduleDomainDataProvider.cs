using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class MonthScheduleDomainDataProvider : IMonthScheduleDomainDataProvider
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ISeatOccupancyProvider _seatBookingProvider;
		private readonly ILicenseAvailability _licenseAvailability;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;

		public MonthScheduleDomainDataProvider(IScheduleProvider scheduleProvider, ISeatOccupancyProvider seatBookingProvider, IPermissionProvider permissionProvider, ILicenseAvailability licenseAvailability, ILoggedOnUser loggedOnUser)
		{
			_scheduleProvider = scheduleProvider;
			_seatBookingProvider = seatBookingProvider;
			_permissionProvider = permissionProvider;
			_licenseAvailability = licenseAvailability;
			_loggedOnUser = loggedOnUser;
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
			var seatBookings = loadSeatBooking ? _seatBookingProvider.GetSeatBookingsForScheduleDays(period, _loggedOnUser.CurrentUser()).ToLookup(k => k.BelongsToDate) : null;

			var days = scheduleDays.Select(scheduleDay =>
			{
				var personAssignment = scheduleDay.PersonAssignment();
				return new MonthScheduleDayDomainData
				{
					SignificantPartForDisplay = scheduleDay.SignificantPartForDisplay(),
					PersonAssignment = personAssignment,
					ScheduleDay = scheduleDay,
					SeatBookingInformation = personAssignment == null
						? null
						: seatBookings?[scheduleDay.DateOnlyAsPeriod.DateOnly].ToArray()
				};
			});

			var asmPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
			return new MonthScheduleDomainData
			{
				CurrentDate = date,
				Days = days,
				AsmEnabled = asmPermission && isAsmLicenseAvailable()
			};
		}
		private bool isAsmLicenseAvailable()
		{
			return _licenseAvailability.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger);
		}
	}
}