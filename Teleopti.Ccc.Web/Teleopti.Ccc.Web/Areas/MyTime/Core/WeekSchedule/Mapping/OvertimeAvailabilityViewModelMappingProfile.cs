using System.Globalization;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class OvertimeAvailabilityViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<IOvertimeAvailability, OvertimeAvailabilityViewModel>()
				.ForMember(d => d.HasOvertimeAvailability, o => o.MapFrom(s => true))
				.ForMember(d => d.StartTime, o => o.MapFrom(s => TimeHelper.TimeOfDayFromTimeSpan(s.StartTime.Value, CultureInfo.CurrentCulture)))
				.ForMember(d => d.EndTime, o => o.MapFrom(s => TimeHelper.TimeOfDayFromTimeSpan(s.EndTime.Value, CultureInfo.CurrentCulture)))
				.ForMember(d => d.NextDay, o => o.MapFrom(s => s.EndTime.Value.Days > 0));
		}
	}
}