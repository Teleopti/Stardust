using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayInputMappingProfile : Profile
	{
		private readonly Func<IShiftCategoryRepository> _shiftCategoryRepository;
		private readonly Func<IDayOffRepository> _dayOffRepository;
		private readonly Func<IAbsenceRepository> _absenceRepository;
		private readonly Func<IActivityRepository> _activityRespository;

		public PreferenceDayInputMappingProfile(Func<IShiftCategoryRepository> shiftCategoryRepository, Func<IDayOffRepository> dayOffRepository, Func<IAbsenceRepository> absenceRepository, Func<IActivityRepository> activityRespository)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
			_dayOffRepository = dayOffRepository;
			_absenceRepository = absenceRepository;
			_activityRespository = activityRespository;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<PreferenceDayInput, IPreferenceDay>()
				.ConvertUsing<PreferenceDayInputToPreferenceDay>()
				;

			CreateMap<PreferenceDayInput, IPreferenceRestriction>()
				.ConstructUsing(s => new PreferenceRestriction())
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => _shiftCategoryRepository.Invoke().Get(s.PreferenceId)))
				.ForMember(d => d.Absence, o => o.MapFrom(s => _absenceRepository.Invoke().Get(s.PreferenceId)))
				.ForMember(d => d.DayOffTemplate, o => o.MapFrom(s => _dayOffRepository.Invoke().Get(s.PreferenceId)))
				.ForMember(d => d.MustHave, o => o.Ignore())
				.ForMember(d => d.ActivityRestrictionCollection, o => o.Ignore())
				.ForMember(d => d.StartTimeLimitation,
				           o =>
				           o.MapFrom(
					           s => new StartTimeLimitation(FromTimeOfDay(s.EarliestStartTime), FromTimeOfDay(s.LatestStartTime))))
				.ForMember(d => d.EndTimeLimitation,
				           o =>
				           o.MapFrom(s => new EndTimeLimitation(FromTimeOfDay(s.EarliestEndTime), FromTimeOfDay(s.LatestEndTime))))
				.ForMember(d => d.WorkTimeLimitation,
				           o => o.MapFrom(s => new WorkTimeLimitation(s.MinimumWorkTime, s.MaximumWorkTime)))
				.AfterMap((s, d) =>
					{
						if (s.ActivityPreferenceId != Guid.Empty)
						{
							IActivityRestriction activityRestriction;
							if (d.ActivityRestrictionCollection.Count == 0)
							{
								activityRestriction = new ActivityRestriction(_activityRespository.Invoke().Get(s.ActivityPreferenceId));
								d.AddActivityRestriction(activityRestriction);
							}
							else
							{
								activityRestriction = d.ActivityRestrictionCollection.First();
								activityRestriction.Activity = _activityRespository.Invoke().Get(s.ActivityPreferenceId);
							}

							activityRestriction.StartTimeLimitation =
								new StartTimeLimitation(FromTimeOfDay(s.ActivityEarliestStartTime), FromTimeOfDay(s.ActivityLatestStartTime));
							activityRestriction.EndTimeLimitation =
								new EndTimeLimitation(FromTimeOfDay(s.ActivityEarliestEndTime), FromTimeOfDay(s.ActivityLatestEndTime));
							activityRestriction.WorkTimeLimitation = new WorkTimeLimitation(s.ActivityMinimumTime, s.ActivityMaximumTime);
						}
						else
						{
							var currentItems = new List<IActivityRestriction>(d.ActivityRestrictionCollection);
							currentItems.ForEach(d.RemoveActivityRestriction);
						}
					})
				;
		}

		private TimeSpan? FromTimeOfDay(TimeOfDay? value)
		{
			if (!value.HasValue) return null;
			return value.Value.Time;
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
				}
				else
				{
					_mapper.Invoke().Map(source, destination.Restriction);
				}
				return destination;
			}
		}
	}
}