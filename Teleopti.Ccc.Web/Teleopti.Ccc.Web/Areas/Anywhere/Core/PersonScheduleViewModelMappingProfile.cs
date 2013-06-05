using System;
using System.Collections.Generic;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMappingProfile : Profile
	{
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IJsonDeserializer _deserializer;

		public PersonScheduleViewModelMappingProfile(IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IJsonDeserializer deserializer)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_deserializer = deserializer;
		}

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
					}));

			CreateMap<IPersonAbsence, PersonScheduleViewModelPersonAbsence>()
				.ForMember(x => x.Color, o => o.ResolveUsing(s => s.Layer.Payload.DisplayColor.ToHtml()))
				.ForMember(x => x.Name, o => o.ResolveUsing(s => s.Layer.Payload.Description.Name))
				.ForMember(x => x.StartTime, o => o.ResolveUsing(s =>
					{
						var timeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
						var absenceStart = s.Layer.Period.StartDateTime;
						var personScheduleDayReadModelOnStartDay =
							s.Person.Id.HasValue
								? _personScheduleDayReadModelRepository.ForPerson(new DateOnly(absenceStart), s.Person.Id.Value)
								: null;
						var shiftStart = getShiftStartOnAbsenceStartDate(personScheduleDayReadModelOnStartDay);

						if (absenceStart == DateTime.MinValue)
							return null;
						var absenceStartLocal = TimeZoneInfo.ConvertTimeFromUtc(absenceStart, timeZoneInfo);
						var shiftStartLocal = TimeZoneInfo.ConvertTimeFromUtc(shiftStart, timeZoneInfo);
						var startTime = absenceStartLocal.Date < shiftStartLocal.Date
							                ? absenceStartLocal.ToFixedDateTimeFormat()
							                : shiftStartLocal.ToFixedDateTimeFormat();
						return startTime;
					}))
				.ForMember(x => x.EndTime, o => o.ResolveUsing(s =>
					{
						var timeZoneInfo = s.Person.PermissionInformation.DefaultTimeZone();
						var absenceEnd = s.Layer.Period.EndDateTime;
						var personScheduleDayReadModelOnEndDay =
							s.Person.Id.HasValue
								? _personScheduleDayReadModelRepository.ForPerson(new DateOnly(absenceEnd), s.Person.Id.Value)
								: null;
						var shiftEnd = getShiftEndOnAbsenceEndDate(personScheduleDayReadModelOnEndDay);
						if (absenceEnd == DateTime.MaxValue)
							return null;
						var absenceEndLocal = TimeZoneInfo.ConvertTimeFromUtc(absenceEnd, timeZoneInfo);
						var shiftEndLocal = TimeZoneInfo.ConvertTimeFromUtc(shiftEnd, timeZoneInfo);
						var endTime = shiftEndLocal.Date < absenceEndLocal.Date
							                 ? absenceEndLocal.ToFixedDateTimeFormat()
							                 : shiftEndLocal.ToFixedDateTimeFormat();
						return endTime;
					}))
				;

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

		private DateTime getShiftStartOnAbsenceStartDate(IPersonScheduleDayReadModel personScheduleDayReadModelOnStartDay)
		{
			DateTime shiftStart = DateTime.MaxValue;
			if (personScheduleDayReadModelOnStartDay != null && personScheduleDayReadModelOnStartDay.Shift != null)
			{
				dynamic shiftOnStartDay =
					_deserializer.DeserializeObject(personScheduleDayReadModelOnStartDay.Shift);
				
				if (shiftOnStartDay != null && shiftOnStartDay.HasUnderlyingShift)
				{
					var projection = shiftOnStartDay.Projection as IEnumerable<dynamic>;
					if (projection != null && !projection.IsEmpty())
					{
						shiftStart = projection.Min(p => p.Start);
					}
				}
			}
			return shiftStart;
		}

		private DateTime getShiftEndOnAbsenceEndDate(IPersonScheduleDayReadModel personScheduleDayReadModelOnEndDay)
		{
			DateTime shiftEnd = DateTime.MinValue;
			if (personScheduleDayReadModelOnEndDay != null && personScheduleDayReadModelOnEndDay.Shift != null)
			{
				dynamic shiftOnEndDay =
					_deserializer.DeserializeObject(personScheduleDayReadModelOnEndDay.Shift);

				if (shiftOnEndDay != null && shiftOnEndDay.HasUnderlyingShift)
				{
					var projection = shiftOnEndDay.Projection as IEnumerable<dynamic>;
					if (projection != null && !projection.IsEmpty())
					{
						shiftEnd = projection.Max(p => p.End);
					}
				}
			}
			return shiftEnd;
		}
	}
}