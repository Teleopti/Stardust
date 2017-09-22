using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayViewModelMapper
	{
		private readonly IStudentAvailabilityProvider _studentAvailabilityProvider;

		public StudentAvailabilityDayViewModelMapper(IStudentAvailabilityProvider studentAvailabilityProvider)
		{
			_studentAvailabilityProvider = studentAvailabilityProvider;
		}

		public StudentAvailabilityDayViewModel Map(IStudentAvailabilityDay s)
		{
			var studentAvailabilityRestriction = _studentAvailabilityProvider.GetStudentAvailabilityForDay(s);
			var availableTimeSpan = studentAvailabilityRestriction == null ? string.Empty : string.Format(
				CultureInfo.InvariantCulture,
				"{0} - {1}", studentAvailabilityRestriction.StartTimeLimitation.StartTimeString,
				studentAvailabilityRestriction.EndTimeLimitation.EndTimeString);
			return new StudentAvailabilityDayViewModel
			{
				Date = s.RestrictionDate.ToFixedClientDateOnlyFormat(),
				AvailableTimeSpan = availableTimeSpan
			};
		}
	}
}