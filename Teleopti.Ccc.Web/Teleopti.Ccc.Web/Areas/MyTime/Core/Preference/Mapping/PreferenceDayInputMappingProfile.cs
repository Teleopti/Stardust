using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayInputMappingProfile : Profile
	{
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IDayOffTemplateRepository _dayOffRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IActivityRepository _activityRespository;
		private readonly Lazy<IMappingEngine> _mapper;

		public PreferenceDayInputMappingProfile(IShiftCategoryRepository shiftCategoryRepository, IDayOffTemplateRepository dayOffRepository, IAbsenceRepository absenceRepository, IActivityRepository activityRespository, Lazy<IMappingEngine> mapper)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
			_dayOffRepository = dayOffRepository;
			_absenceRepository = absenceRepository;
			_activityRespository = activityRespository;
			_mapper = mapper;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<PreferenceDayInput, IPreferenceDay>()
				.ConvertUsing<PreferenceDayInputToPreferenceDay>()
				;

			CreateMap<PreferenceDayInput, ActivityRestriction>()
				.ForMember(d => d.Activity, o => o.MapFrom(s => _activityRespository.Get(s.ActivityPreferenceId.Value)))
				.ForMember(d => d.StartTimeLimitation, o => o.ResolveUsing(s =>
							   new StartTimeLimitation(s.ActivityEarliestStartTime.ToTimeSpan(), s.ActivityLatestStartTime.ToTimeSpan())
							   ))
				.ForMember(d => d.EndTimeLimitation, o => o.ResolveUsing(s =>
							   new EndTimeLimitation(s.ActivityEarliestEndTime.ToTimeSpan(), s.ActivityLatestEndTime.ToTimeSpan())
							   ))
				.ForMember(d => d.WorkTimeLimitation, o => o.ResolveUsing(
							   s => new WorkTimeLimitation(s.ActivityMinimumTime, s.ActivityMaximumTime)
							   ))
							   .ForMember(d => d.Parent, o => o.Ignore())
				;

			CreateMap<PreferenceDayInput, IPreferenceRestriction>()
				.ConstructUsing((PreferenceDayInput s) => new PreferenceRestriction())
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => s.PreferenceId != null ? _shiftCategoryRepository.Get(s.PreferenceId.Value) : null))
				.ForMember(d => d.Absence, o => o.MapFrom(s => s.PreferenceId != null ? _absenceRepository.Get(s.PreferenceId.Value) : null))
				.ForMember(d => d.DayOffTemplate, o => o.MapFrom(s => s.PreferenceId != null ? _dayOffRepository.Get(s.PreferenceId.Value) : null))
				.ForMember(d => d.MustHave, o => o.Ignore())
				.ForMember(d => d.ActivityRestrictionCollection, o => o.Ignore())
				.ForMember(d => d.StartTimeLimitation, o => o.ResolveUsing(s =>
							   new StartTimeLimitation(s.EarliestStartTime.ToTimeSpan(), s.LatestStartTime.ToTimeSpan())
							   ))
				.ForMember(d => d.EndTimeLimitation, o => o.ResolveUsing(s =>
							   new EndTimeLimitation(s.EarliestEndTime.ToTimeSpan(s.EarliestEndTimeNextDay), s.LatestEndTime.ToTimeSpan(s.LatestEndTimeNextDay))
							   ))
				.ForMember(d => d.WorkTimeLimitation, o => o.ResolveUsing(
							   s => new WorkTimeLimitation(s.MinimumWorkTime, s.MaximumWorkTime)
							   ))
				.AfterMap((s, d) =>
					{
						if (s.ActivityPreferenceId.HasValue)
						{
							if (d.ActivityRestrictionCollection.Any())
							{
								var activityRestriction = d.ActivityRestrictionCollection.Cast<ActivityRestriction>().Single();
								_mapper.Value.Map(s, activityRestriction);
							}
							else
							{
								var activityRestriction = _mapper.Value.Map<PreferenceDayInput, ActivityRestriction>(s);
								d.AddActivityRestriction(activityRestriction);
							}
						}
						else
						{
							var currentItems = new List<IActivityRestriction>(d.ActivityRestrictionCollection);
							currentItems.ForEach(d.RemoveActivityRestriction);
						}
					})
				;
		}

		public class PreferenceDayInputToPreferenceDay : ITypeConverter<PreferenceDayInput, IPreferenceDay>
		{
			private readonly Func<ILoggedOnUser> _loggedOnUser;
			private readonly Func<IMappingEngine> _mapper;

			public PreferenceDayInputToPreferenceDay(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser)
			{
				_loggedOnUser = loggedOnUser;
				_mapper = mapper;
			}

			public IPreferenceDay Convert(ResolutionContext context)
			{
				var source = context.SourceValue as PreferenceDayInput;
				var destination = context.DestinationValue as IPreferenceDay;
				if (destination == null)
				{
					var person = _loggedOnUser.Invoke().CurrentUser();
					var restriction = _mapper.Invoke().Map<PreferenceDayInput, IPreferenceRestriction>(source);
					destination = new PreferenceDay(person, source.Date, restriction);
					destination.TemplateName = source.TemplateName;
				}
				else
				{
					destination.TemplateName = source.TemplateName;
					_mapper.Invoke().Map(source, destination.Restriction);
				}
				return destination;
			}
		}
	}

	public static class Extensions
	{
		public static TimeSpan? ToTimeSpan(this TimeOfDay? value)
		{
			if (!value.HasValue) return null;
			return value.Value.Time;
		}

		public static TimeSpan? ToTimeSpan(this TimeOfDay? value, bool nextDay)
		{
			if (!value.HasValue) return null;
			return nextDay ? value.Value.Time.Add(TimeSpan.FromDays(1)) : value.Value.Time;
		}

	}
}