using System;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleDayViewModelMappingProfile : Profile
	{
		private readonly IUserTimeZone _userTimeZone;
		public PersonScheduleDayViewModelMappingProfile(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		protected override void Configure()
		{
			CreateMap<PersonScheduleDayReadModel, PersonScheduleDayViewModel>()
				.ForMember(x => x.Date, o => o.MapFrom(s => s.Date))
				.ForMember(x => x.Person, o => o.MapFrom(s => s.PersonId))
				.ForMember(x => x.StartTime, o => o.ResolveUsing(s =>
				{
					var startTime = s.Start;
					if (s.Start != null)
					{
						startTime = TimeZoneHelper.ConvertFromUtc((DateTime) s.Start, _userTimeZone.TimeZone());
					}
					return startTime;
				}))
				.ForMember(x => x.EndTime, o => o.ResolveUsing(s =>
				{
					var endTime = s.End;
					if (s.End != null)
					{
						endTime = TimeZoneHelper.ConvertFromUtc((DateTime) s.End, _userTimeZone.TimeZone());
					}
					return endTime;
				}))
            .ForMember(x => x.IsDayOff, o => o.MapFrom(s => s.IsDayOff));
		}
	}
}