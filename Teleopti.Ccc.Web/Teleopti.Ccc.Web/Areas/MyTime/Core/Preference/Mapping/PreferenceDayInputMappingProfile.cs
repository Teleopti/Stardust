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

		public PreferenceDayInputMappingProfile(Func<IShiftCategoryRepository> shiftCategoryRepository, Func<IDayOffRepository> dayOffRepository, Func<IAbsenceRepository> absenceRepository)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
			_dayOffRepository = dayOffRepository;
			_absenceRepository = absenceRepository;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<PreferenceDayInput, IPreferenceDay>()
				.ConvertUsing<PreferenceDayInputToPreferenceDay>()
				;

			CreateMap<PreferenceDayInput, IPreferenceRestriction>()
				.ConstructUsing(s => new PreferenceRestriction())
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => _shiftCategoryRepository.Invoke().Get(s.Id)))
				.ForMember(d => d.Absence, o => o.MapFrom(s => _absenceRepository.Invoke().Get(s.Id)))
				.ForMember(d => d.DayOffTemplate, o => o.MapFrom(s => _dayOffRepository.Invoke().Get(s.Id)))
				.ForMember(d => d.ActivityRestrictionCollection, o => o.Ignore())
				.ForMember(d => d.MustHave, o => o.Ignore())
				.ForMember(d => d.StartTimeLimitation, o => o.Ignore())
				.ForMember(d => d.EndTimeLimitation, o => o.Ignore())
				.ForMember(d => d.WorkTimeLimitation, o => o.Ignore())
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