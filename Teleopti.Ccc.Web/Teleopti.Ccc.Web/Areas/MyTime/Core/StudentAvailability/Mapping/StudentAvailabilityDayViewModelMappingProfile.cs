using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			CreateMap<IStudentAvailabilityRestriction, StudentAvailabilityDayViewModel>()
				.ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTimeLimitation.StartTimeString))
				.ForMember(d => d.EndTime, o => o.MapFrom(s =>
				                                          	{
				                                          		if (!s.EndTimeLimitation.EndTime.HasValue)
				                                          			return string.Empty;
				                                          		var timeOfDay = TimeHelper.ParseTimeOfDayFromTimeSpan(s.EndTimeLimitation.EndTime.Value).TimeOfDay;
				                                          		return TimeHelper.TimeOfDayFromTimeSpan(timeOfDay);
				                                          	}))
				.ForMember(d => d.NextDay, o => o.MapFrom(s =>
				                                          	{
				                                          		if (!s.EndTimeLimitation.EndTime.HasValue)
				                                          			return false;
				                                          		return TimeHelper.ParseTimeOfDayFromTimeSpan(s.EndTimeLimitation.EndTime.Value).Days == 1;
				                                          	})
				);
		}
	}
}