using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayViewModelMapper
	{
		private readonly IStudentAvailabilityProvider _studentAvailabilityProvider;
		private readonly BankHolidayCalendarViewModelMapper _bankHolidayCalendarViewModelMapper;

		public StudentAvailabilityDayViewModelMapper(IStudentAvailabilityProvider studentAvailabilityProvider, 
			BankHolidayCalendarViewModelMapper bankHolidayCalendarViewModelMapper)
		{
			_studentAvailabilityProvider = studentAvailabilityProvider;
			_bankHolidayCalendarViewModelMapper = bankHolidayCalendarViewModelMapper;
		}

		public StudentAvailabilityDayViewModel Map(IStudentAvailabilityDay s, IBankHolidayDate bankHolidayDate)
		{
			var studentAvailabilityRestriction = _studentAvailabilityProvider.GetStudentAvailabilityForDay(s);
			var availableTimeSpan = studentAvailabilityRestriction == null ? string.Empty : string.Format(
				CultureInfo.InvariantCulture,
				"{0} - {1}", studentAvailabilityRestriction.StartTimeLimitation.StartTimeString,
				studentAvailabilityRestriction.EndTimeLimitation.EndTimeString);
			return new StudentAvailabilityDayViewModel
			{
				Date = s.RestrictionDate.ToFixedClientDateOnlyFormat(),
				AvailableTimeSpan = availableTimeSpan,
				BankHolidayCalendar = _bankHolidayCalendarViewModelMapper.Map(bankHolidayDate)
			};
		}
	}
}