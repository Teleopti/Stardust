using AutoMapper;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class OvertimeAvailabilityInputMappingProfile : Profile
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public OvertimeAvailabilityInputMappingProfile(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<OvertimeAvailabilityInput, IOvertimeAvailability>()
				.ConstructUsing(
					s =>
					new OvertimeAvailability(_loggedOnUser.CurrentUser(), s.Date, s.StartTime.ToTimeSpan(),
					                         s.EndTime.ToTimeSpan(s.EndTimeNextDay)))
				.ForMember(d => d.NotAvailable, o => o.Ignore());
		}
	}
}