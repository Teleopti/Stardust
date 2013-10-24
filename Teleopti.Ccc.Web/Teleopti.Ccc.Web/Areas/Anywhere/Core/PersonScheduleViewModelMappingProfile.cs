using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
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
				.ForMember(x => x.IsDayOff, o => o.MapFrom(s=> s.Model.DayOff != null))
				.ForMember(x => x.IsFullDayAbsence, o => o.MapFrom(s => s.Model.Shift.IsFullDayAbsence))
				.ForMember(x => x.Layers, o => o.ResolveUsing(s => from layer in s.Model.Shift.Projection
				                                                   select new PersonScheduleViewModelLayer
					                                                   {
						                                                   Color = s.HasViewConfidentialPermission || !layer.IsAbsenceConfidential
							                                                           ? layer.Color
							                                                           : ConfidentialPayloadValues.DisplayColor.ToHtml(),
						                                                   Start = layer.Start == DateTime.MinValue
							                                                           ? null
							                                                           : TimeZoneInfo.ConvertTimeFromUtc(layer.Start, s.Person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat(),
						                                                   Minutes = layer.Minutes,
						                                                   Description = layer.Description
					                                                   }))
				.ForMember(x => x.PersonAbsences, o => o.ResolveUsing(
					s => (from personAbsence in s.PersonAbsences
						  let timeZoneInfo = personAbsence.Person.PermissionInformation.DefaultTimeZone()
						  let startTime = personAbsence.Layer.Period.StartDateTime == DateTime.MinValue
								  ? null
								  : TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Layer.Period.StartDateTime,timeZoneInfo).ToFixedDateTimeFormat()
						  let endTime = personAbsence.Layer.Period.EndDateTime == DateTime.MinValue
								  ? null
								  : TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Layer.Period.EndDateTime, timeZoneInfo).ToFixedDateTimeFormat()
						  select new PersonScheduleViewModelPersonAbsence
						  {
							  Id = personAbsence.Id.ToString(),
							  Color = s.HasViewConfidentialPermission || !personAbsence.Layer.Payload.Confidential
									  ? personAbsence.Layer.Payload.DisplayColor.ToHtml()
									  : ConfidentialPayloadValues.DisplayColor.ToHtml(),
							  Name = s.HasViewConfidentialPermission || !personAbsence.Layer.Payload.Confidential
									  ? personAbsence.Layer.Payload.Description.Name
									  : ConfidentialPayloadValues.Description.Name,
							  StartTime = startTime,
							  EndTime = endTime
						  }).ToArray()))
				;

			CreateMap<IAbsence, PersonScheduleViewModelAbsence>();

		}
	}
}