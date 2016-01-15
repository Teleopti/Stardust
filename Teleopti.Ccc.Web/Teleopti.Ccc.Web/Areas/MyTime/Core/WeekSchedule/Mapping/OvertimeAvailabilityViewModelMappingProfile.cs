using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class OvertimeAvailabilityViewModelMappingProfile : Profile
	{
		private readonly IUserCulture _culture;

		public OvertimeAvailabilityViewModelMappingProfile(IUserCulture culture)
		{
			_culture = culture;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IOvertimeAvailability, OvertimeAvailabilityViewModel>()
				.ForMember(d => d.HasOvertimeAvailability, o => o.MapFrom(s => true))
				.ForMember(d => d.StartTime, o => o.MapFrom(s => TimeHelper.TimeOfDayFromTimeSpan(s.StartTime.Value, _culture.GetCulture())))
				.ForMember(d => d.EndTime, o => o.MapFrom(s => TimeHelper.TimeOfDayFromTimeSpan(s.EndTime.Value, _culture.GetCulture())))
				.ForMember(d => d.EndTimeNextDay, o => o.MapFrom(s => s.EndTime.Value.Days > 0))
				.ForMember(d => d.DefaultStartTime, c => c.Ignore())
				.ForMember(d => d.DefaultEndTime, c => c.Ignore())
				.ForMember(d => d.DefaultEndTimeNextDay, c => c.Ignore());
		}
	}
}