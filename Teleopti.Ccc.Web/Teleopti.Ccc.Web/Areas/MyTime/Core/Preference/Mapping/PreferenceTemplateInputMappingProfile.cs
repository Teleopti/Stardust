using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceTemplateInputMappingProfile : Profile
	{
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IDayOffTemplateRepository _dayOffRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IActivityRepository _activityRespository;
		private readonly Lazy<IMappingEngine> _mapper;

		public PreferenceTemplateInputMappingProfile(IShiftCategoryRepository shiftCategoryRepository, IDayOffTemplateRepository dayOffRepository, IAbsenceRepository absenceRepository, IActivityRepository activityRespository, Lazy<IMappingEngine> mapper)
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

			CreateMap<PreferenceTemplateInput, IExtendedPreferenceTemplate>()
				.ConvertUsing<PreferenceTemplateInputToExtendedPreferenceTemplate>()
				;

			CreateMap<PreferenceTemplateInput, ActivityRestrictionTemplate>()
				.ForMember(d => d.Activity, o => o.MapFrom(s => _activityRespository.Get(s.ActivityPreferenceId.Value)))
				.ForMember(d => d.Parent, o => o.Ignore())
				.ForMember(d => d.StartTimeLimitation, o => o.MapFrom(s =>
							   new StartTimeLimitation(s.ActivityEarliestStartTime.ToTimeSpan(), s.ActivityLatestStartTime.ToTimeSpan())
							   ))
				.ForMember(d => d.EndTimeLimitation, o => o.MapFrom(s =>
							   new EndTimeLimitation(s.ActivityEarliestEndTime.ToTimeSpan(), s.ActivityLatestEndTime.ToTimeSpan())
							   ))
				.ForMember(d => d.WorkTimeLimitation, o => o.MapFrom(
							   s => new WorkTimeLimitation(s.ActivityMinimumTime, s.ActivityMaximumTime)
							   ))
				.ForMember(d => d.Parent, o => o.Ignore())
				;

			CreateMap<PreferenceTemplateInput, IPreferenceRestrictionTemplate>()
				.ConstructUsing((ResolutionContext c) => new PreferenceRestrictionTemplate())
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => s.PreferenceId != null ? _shiftCategoryRepository.Get(s.PreferenceId.Value) : null))
				.ForMember(d => d.Absence, o => o.MapFrom(s => s.PreferenceId != null ? _absenceRepository.Get(s.PreferenceId.Value) : null))
				.ForMember(d => d.DayOffTemplate, o => o.MapFrom(s => s.PreferenceId != null ? _dayOffRepository.Get(s.PreferenceId.Value) : null))
				.ForMember(d => d.MustHave, o => o.Ignore())
				.ForMember(d => d.ActivityRestrictionCollection, o => o.Ignore())
				.ForMember(d => d.StartTimeLimitation, o => o.MapFrom(s =>
							   new StartTimeLimitation(s.EarliestStartTime.ToTimeSpan(), s.LatestStartTime.ToTimeSpan())
							   ))
				.ForMember(d => d.EndTimeLimitation, o => o.MapFrom(s =>
							   new EndTimeLimitation(s.EarliestEndTime.ToTimeSpan(s.EarliestEndTimeNextDay), s.LatestEndTime.ToTimeSpan(s.LatestEndTimeNextDay))
							   ))
				.ForMember(d => d.WorkTimeLimitation, o => o.MapFrom(
							   s => new WorkTimeLimitation(s.MinimumWorkTime, s.MaximumWorkTime)
							   ))
				.AfterMap((s, d) =>
				{
					if (s.ActivityPreferenceId.HasValue)
					{
						if (d.ActivityRestrictionCollection.Any())
						{
							var activityRestriction = d.ActivityRestrictionCollection.Cast<ActivityRestrictionTemplate>().Single();
							_mapper.Value.Map(s, activityRestriction);
						}
						else
						{
							var activityRestriction = _mapper.Value.Map<PreferenceTemplateInput, ActivityRestrictionTemplate>(s);
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

		public class PreferenceTemplateInputToExtendedPreferenceTemplate : ITypeConverter<PreferenceTemplateInput, IExtendedPreferenceTemplate>
		{
			private readonly Func<IMappingEngine> _mapper;
			private readonly Func<ILoggedOnUser> _loggedOnUser;

			public PreferenceTemplateInputToExtendedPreferenceTemplate(Func<IMappingEngine> mapper, Func<ILoggedOnUser> loggedOnUser)
			{
				_mapper = mapper;
				_loggedOnUser = loggedOnUser;
			}

			public IExtendedPreferenceTemplate Convert(ResolutionContext context)
			{
				var source = context.SourceValue as PreferenceTemplateInput;
				var destination = context.DestinationValue as IExtendedPreferenceTemplate;
				if (destination == null && source != null)
				{
					var person = _loggedOnUser.Invoke().CurrentUser();
					var restrictionTemplate = _mapper.Invoke().Map<PreferenceTemplateInput, IPreferenceRestrictionTemplate>(source);
					destination = new ExtendedPreferenceTemplate(person, restrictionTemplate, source.NewTemplateName, Color.White);
				}
				else
				{
					throw new NotSupportedException("update preference template not support now");
				}
				return destination;
			}
		}
	}
}