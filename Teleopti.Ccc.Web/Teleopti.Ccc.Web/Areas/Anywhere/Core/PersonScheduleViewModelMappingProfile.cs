using System.Collections.Generic;
using AutoMapper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			CreateMap<PersonScheduleData, PersonScheduleViewModel>()
				.ForMember(x => x.Name, o => o.MapFrom(s => s.Person.Name.ToString()))
				.ForMember(x => x.Team, o => o.MapFrom(s => s.Person.MyTeam(new DateOnly(s.Date)).Description.Name))
				.ForMember(x => x.Site, o => o.MapFrom(s => s.Person.MyTeam(new DateOnly(s.Date)).Site.Description.Name))
				.ForMember(x => x.Layers, o => o.MapFrom(s =>
					{
						if (s.Shift == null)
							return null;
						return s.Shift.Projection as IEnumerable<dynamic>;
					}))
				;

			CreateMap<dynamic, PersonScheduleViewModelLayer>()
				.ForMember(x => x.Color, o => o.MapFrom(s => s.Color))
				.ForMember(x => x.Start, o => o.MapFrom(s => s.Start))
				.ForMember(x => x.Minutes, o => o.MapFrom(s => s.Minutes))
				;

		}
	}
}