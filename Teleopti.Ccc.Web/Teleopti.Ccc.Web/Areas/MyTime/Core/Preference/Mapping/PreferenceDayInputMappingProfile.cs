using System;
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
				.ConstructUsing(s =>
					{
						var restriction = new PreferenceRestriction();
						if (s.ActivityPreferenceId != Guid.Empty)
						{
							restriction.AddActivityRestriction(
								new ActivityRestriction(_activityRespository.Invoke().Get(s.ActivityPreferenceId))
									{
										StartTimeLimitation = new StartTimeLimitation(s.ActivityEarliestStartTime, s.ActivityLatestStartTime),
										EndTimeLimitation = new EndTimeLimitation(s.ActivityEarliestEndTime, s.ActivityLatestEndTime),
										WorkTimeLimitation = new WorkTimeLimitation(s.ActivityMinimumTime, s.ActivityMaximumTime)
									});
						}
						return restriction;
					})
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => _shiftCategoryRepository.Invoke().Get(s.PreferenceId)))
				.ForMember(d => d.Absence, o => o.MapFrom(s => _absenceRepository.Invoke().Get(s.PreferenceId)))
				.ForMember(d => d.DayOffTemplate, o => o.MapFrom(s => _dayOffRepository.Invoke().Get(s.PreferenceId)))
				.ForMember(d => d.MustHave, o => o.Ignore())
				.ForMember(d => d.ActivityRestrictionCollection, o => o.Ignore())
				.ForMember(d => d.StartTimeLimitation, o => o.MapFrom(s => new StartTimeLimitation(s.EarliestStartTime,s.LatestStartTime)))
				.ForMember(d => d.EndTimeLimitation, o => o.MapFrom(s => new EndTimeLimitation(s.EarliestEndTime,s.LatestEndTime)))
				.ForMember(d => d.WorkTimeLimitation, o => o.MapFrom(s => new WorkTimeLimitation(s.MinimumWorkTime,s.MaximumWorkTime)))
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