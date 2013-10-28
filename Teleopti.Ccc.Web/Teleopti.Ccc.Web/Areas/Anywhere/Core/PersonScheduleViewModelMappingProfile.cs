using System;
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
		private class MapContext<TParent, TChild>
		{
			public MapContext(TParent parent, TChild child)
			{
				Parent = parent;
				Child = child;
			}

			public readonly TParent Parent;
			public readonly TChild Child;
		}

		protected override void Configure()
		{
			CreateMap<PersonScheduleData, PersonScheduleViewModel>()
				.ForMember(x => x.Name, o => o.MapFrom(s => s.Person.Name.ToString()))
				.ForMember(x => x.Team, o => o.MapFrom(s => s.Person.MyTeam(new DateOnly(s.Date)).Description.Name))
				.ForMember(x => x.Site, o => o.MapFrom(s => s.Person.MyTeam(new DateOnly(s.Date)).Site.Description.Name))
				.ForMember(x => x.IsDayOff, o => o.MapFrom(s => s.Model.DayOff != null))
				.ForMember(x => x.IsFullDayAbsence, o => o.MapFrom(s => s.Model.Shift.IsFullDayAbsence))
				.ForMember(x => x.Layers, o => o.MapFrom(s => from p in s.Model.Shift.Projection ?? new SimpleLayer[] {}
				                                              select new MapContext<PersonScheduleData, SimpleLayer>(s, p)
					                               ))
				.ForMember(x => x.PersonAbsences, o => o.MapFrom(
					s => from p in s.PersonAbsences ?? new IPersonAbsence[] {}
					     select new MapContext<PersonScheduleData, IPersonAbsence>(s, p)
					                                       ))
				;

			CreateMap<MapContext<PersonScheduleData, SimpleLayer>, PersonScheduleViewModelLayer>()
				.ForMember(x => x.Start, o => o.MapFrom(s => TimeZoneInfo.ConvertTimeFromUtc(s.Child.Start, s.Parent.Person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat()))
				.ForMember(x => x.Minutes, o => o.MapFrom(s => s.Child.Minutes))
				.ForMember(x => x.Color, o => o.MapFrom(s => (s.Child.IsAbsenceConfidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.DisplayColor.ToHtml() : s.Child.Color))
				.ForMember(x => x.Description, o => o.MapFrom(s => (s.Child.IsAbsenceConfidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.Description.Name : s.Child.Description))
				;

			CreateMap<MapContext<PersonScheduleData, IPersonAbsence>, PersonScheduleViewModelPersonAbsence>()
				.ForMember(x => x.Id, o => o.MapFrom(s => s.Child.Id))
				.ForMember(x => x.StartTime, o => o.MapFrom(s => TimeZoneInfo.ConvertTimeFromUtc(s.Child.Layer.Period.StartDateTime, s.Child.Person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat()))
				.ForMember(x => x.EndTime, o => o.MapFrom(s => TimeZoneInfo.ConvertTimeFromUtc(s.Child.Layer.Period.EndDateTime, s.Child.Person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat()))
				.ForMember(x => x.Name, o => o.MapFrom(s => (s.Child.Layer.Payload.Confidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.Description.Name : s.Child.Layer.Payload.Description.Name))
				.ForMember(x => x.Color, o => o.MapFrom(s => (s.Child.Layer.Payload.Confidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.DisplayColor.ToHtml() : s.Child.Layer.Payload.DisplayColor.ToHtml()))
				;

			CreateMap<IAbsence, PersonScheduleViewModelAbsence>();

		}
	}
}