using System;
using System.Collections.Generic;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Interfaces.Domain;
using System.Linq;

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
				.ForMember(x => x.Layers, o => o.ResolveUsing(s =>
					{
						if (s.Shift == null)
							return null;
						var layers = s.Shift.Projection as IEnumerable<dynamic>;
						layers.ForEach(l =>
							{
								if (s.Person == null)
									return;
								l.TimeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
							});
						return layers;
					}))
				.ForMember(x => x.PersonAbsences, o => o.ResolveUsing(s =>
					{
						var shift=s.Shift;

						DateTime minimumTime = DateTime.MinValue;
						DateTime maximumTime = DateTime.MaxValue;

						if (shift != null)
						{
							var projection = s.Shift.Projection as IEnumerable<dynamic>;
							if (projection != null)
							{
								minimumTime = projection.Min(p => p.Start);
								maximumTime = projection.Max(p => p.End);
							}
						}
						
						var res = new List<PersonScheduleViewModelPersonAbsence>();
						foreach (var absence in s.PersonAbsences)
						{
							var timeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
							var personAbsence = new PersonScheduleViewModelPersonAbsence
								{
									Id = absence.Id.ToString(),
									Color = absence.Layer.Payload.DisplayColor.ToHtml(),
									Name = absence.Layer.Payload.Description.Name,
									StartTime =
										minimumTime ==
										DateTime.MinValue
											? null
											: TimeZoneInfo.ConvertTimeFromUtc(
												minimumTime, timeZoneInfo)
											              .ToFixedDateTimeFormat(),
									EndTime =
										maximumTime == DateTime.MinValue
											? null
											: TimeZoneInfo.ConvertTimeFromUtc(
												maximumTime, timeZoneInfo)
											              .ToFixedDateTimeFormat()
								};
							res.Add(personAbsence);
						}
						return res;
					}));

			//CreateMap<IPersonAbsence, PersonScheduleViewModelPersonAbsence>()
			//	.ForMember(x => x.Color, o => o.ResolveUsing(s => s.Layer.Payload.DisplayColor.ToHtml()))
			//	.ForMember(x => x.Name, o => o.ResolveUsing(s => s.Layer.Payload.Description.Name))
			//	.ForMember(x => x.StartTime, o => o.ResolveUsing(s =>
			//		{
			//			if (s.Layer.Period.StartDateTime == DateTime.MinValue)
			//				return null;
			//			TimeZoneInfo timeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
			//			return TimeZoneInfo.ConvertTimeFromUtc(s.Layer.Period.StartDateTime, timeZoneInfo).ToFixedDateTimeFormat();
			//		}))
			//	.ForMember(x => x.EndTime, o => o.ResolveUsing(s =>
			//		{
			//			if (s.Layer.Period.EndDateTime == DateTime.MinValue)
			//				return null;
			//			TimeZoneInfo timeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
			//			return TimeZoneInfo.ConvertTimeFromUtc(s.Layer.Period.EndDateTime, timeZoneInfo).ToFixedDateTimeFormat();
			//		}))
			//	;

			CreateMap<IAbsence, PersonScheduleViewModelAbsence>();
			
			CreateMap<dynamic, PersonScheduleViewModelLayer>()
				.ForMember(x => x.Color, o => o.ResolveUsing(s => s.Color))
				.ForMember(x => x.Start, o => o.ResolveUsing(s =>
					{
						if (s.Start == null)
							return null;
						DateTime start = s.Start;
						if (start == DateTime.MinValue)
							return null;
						TimeZoneInfo timeZoneInfo = s.TimeZoneInfo;
						return TimeZoneInfo.ConvertTimeFromUtc(start, timeZoneInfo).ToFixedDateTimeFormat();
					}))
				.ForMember(x => x.Minutes, o => o.ResolveUsing(s => s.Minutes))
				;

		}
	}
}