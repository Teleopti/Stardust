using System;
using System.Collections.Generic;
using System.Globalization;
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
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;

		public PersonScheduleViewModelMappingProfile(IUserTimeZone userTimeZone, INow now)
		{
			_userTimeZone = userTimeZone;
			_now = now;
		}

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

		private static DateTime roundUp(DateTime dt, TimeSpan d)
		{
			return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
		}

		protected override void Configure()
		{
			CreateMap<PersonScheduleData, PersonScheduleViewModel>()
				.ForMember(x => x.Name, o => o.MapFrom(s => s.Person.Name.ToString()))
				.ForMember(x => x.PersonAbsences, o => o.MapFrom(
					s => from p in s.PersonAbsences ?? new IPersonAbsence[] {}
					     select new MapContext<PersonScheduleData, IPersonAbsence>(s, p)))
				.ForMember(x => x.DefaultIntradayAbsenceData,
						   o => o.MapFrom(s => s.Model.Shift.Projection))
                .ForMember(x => x.TimeZoneName, o => o.MapFrom(s => s.Person.PermissionInformation.DefaultTimeZone().DisplayName))
				;

			CreateMap<IList<SimpleLayer>, DefaultIntradayAbsenceViewModel>()
				.ForMember(x => x.StartTime, o => o.ResolveUsing(s =>
					{
						if (s.FirstOrDefault() == null || s.LastOrDefault() == null)
							return TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.Zero, CultureInfo.CurrentCulture);
						var now = _now.UtcDateTime();
						if (now >= s.FirstOrDefault().Start && now <= s.LastOrDefault().End)
							return TimeHelper.TimeOfDayFromTimeSpan(TimeZoneInfo.ConvertTimeFromUtc(roundUp(now, TimeSpan.FromMinutes(15)), _userTimeZone.TimeZone()).TimeOfDay, CultureInfo.CurrentCulture);
						return TimeHelper.TimeOfDayFromTimeSpan(s.FirstOrDefault() != null
							       ? TimeZoneInfo.ConvertTimeFromUtc(s.FirstOrDefault().Start, _userTimeZone.TimeZone()).TimeOfDay
								   : TimeSpan.Zero, CultureInfo.CurrentCulture);
					}))
				.ForMember(x => x.EndTime, o => o.ResolveUsing(s =>
					{
						var now = _now.UtcDateTime();
						if (s.FirstOrDefault() == null || s.LastOrDefault() == null)
							return TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.Zero, CultureInfo.CurrentCulture);
						if (now >= s.FirstOrDefault().Start && now <= s.LastOrDefault().End)
							return TimeHelper.TimeOfDayFromTimeSpan(TimeZoneInfo.ConvertTimeFromUtc(s.LastOrDefault().End, _userTimeZone.TimeZone()).TimeOfDay, CultureInfo.CurrentCulture);
						return TimeHelper.TimeOfDayFromTimeSpan(s.FirstOrDefault() != null
							       ? TimeZoneInfo.ConvertTimeFromUtc(s.FirstOrDefault().Start, _userTimeZone.TimeZone()).AddHours(1).TimeOfDay
							       : TimeSpan.Zero, CultureInfo.CurrentCulture);
					}))
				;

			CreateMap<MapContext<PersonScheduleData, SimpleLayer>, PersonScheduleViewModelLayer>()
				.ForMember(x => x.Start, o => o.MapFrom(s => TimeZoneInfo.ConvertTimeFromUtc(s.Child.Start, _userTimeZone.TimeZone()).ToFixedDateTimeFormat()))
				.ForMember(x => x.Minutes, o => o.MapFrom(s => s.Child.Minutes))
				.ForMember(x => x.Color, o => o.MapFrom(s => (s.Child.IsAbsenceConfidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.DisplayColorHex : s.Child.Color))
				.ForMember(x => x.Description, o => o.MapFrom(s => (s.Child.IsAbsenceConfidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.Description.Name : s.Child.Description))
				;

			CreateMap<MapContext<PersonScheduleData, IPersonAbsence>, PersonScheduleViewModelPersonAbsence>()
				.ForMember(x => x.Id, o => o.MapFrom(s => s.Child.Id))
				.ForMember(x => x.StartTime, o => o.MapFrom(s => TimeZoneInfo.ConvertTimeFromUtc(s.Child.Layer.Period.StartDateTime, _userTimeZone.TimeZone()).ToFixedDateTimeFormat()))
				.ForMember(x => x.EndTime, o => o.MapFrom(s => TimeZoneInfo.ConvertTimeFromUtc(s.Child.Layer.Period.EndDateTime, _userTimeZone.TimeZone()).ToFixedDateTimeFormat()))
				.ForMember(x => x.Name, o => o.MapFrom(s => (s.Child.Layer.Payload.Confidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.Description.Name : s.Child.Layer.Payload.Description.Name))
				.ForMember(x => x.Color, o => o.MapFrom(s => (s.Child.Layer.Payload.Confidential && !s.Parent.HasViewConfidentialPermission) ? ConfidentialPayloadValues.DisplayColorHex : s.Child.Layer.Payload.DisplayColor.ToHtml()))
				;

			CreateMap<IActivity, PersonScheduleViewModelActivity>();
			CreateMap<IAbsence, PersonScheduleViewModelAbsence>();
		}
	}
}