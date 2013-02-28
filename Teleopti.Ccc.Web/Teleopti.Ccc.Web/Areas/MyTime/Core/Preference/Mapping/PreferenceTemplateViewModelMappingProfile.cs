using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceTemplateViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<IExtendedPreferenceTemplate, PreferenceTemplateViewModel>()
				.ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
				.ForMember(d => d.PreferenceId, o => o.MapFrom(s =>
				{
					if (s.Restriction.DayOffTemplate != null)
						return s.Restriction.DayOffTemplate.Id;
					if (s.Restriction.Absence != null)
						return s.Restriction.Absence.Id;
					if (s.Restriction.ShiftCategory != null)
						return s.Restriction.ShiftCategory.Id;
					return null;
				}))
				.ForMember(d => d.EarliestStartTime, o => o.MapFrom(s => s.Restriction.StartTimeLimitation.StartTimeString))
				.ForMember(d => d.LatestStartTime, o => o.MapFrom(s => s.Restriction.StartTimeLimitation.EndTimeString))
				.ForMember(d => d.EarliestEndTime, o => o.MapFrom(s => s.Restriction.EndTimeLimitation.StartTimeString))
				.ForMember(d => d.LatestEndTime, o => o.MapFrom(s => s.Restriction.EndTimeLimitation.EndTimeString))
				.ForMember(d => d.MinimumWorkTime, o => o.MapFrom(s => s.Restriction.WorkTimeLimitation.StartTimeString))
				.ForMember(d => d.MaximumWorkTime, o => o.MapFrom(s => s.Restriction.WorkTimeLimitation.EndTimeString))
				.ForMember(d => d.ActivityPreferenceId, o => o.MapFrom(s =>GetActivityRestrictionValue(s, r => r.Activity.Id)))
				.ForMember(d => d.ActivityEarliestStartTime, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.StartTimeLimitation.StartTimeString)))
				.ForMember(d => d.ActivityLatestStartTime, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.StartTimeLimitation.EndTimeString)))
				.ForMember(d => d.ActivityEarliestEndTime, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.EndTimeLimitation.StartTimeString)))
				.ForMember(d => d.ActivityLatestEndTime, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.EndTimeLimitation.EndTimeString)))
				.ForMember(d => d.ActivityMinimumTime, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.WorkTimeLimitation.StartTimeString)))
				.ForMember(d => d.ActivityMaximumTime, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.WorkTimeLimitation.EndTimeString)))
				;
		}

		private T GetActivityRestrictionValue<T>(IExtendedPreferenceTemplate template, Func<IActivityRestriction, T> getter)
		{
			if (template.Restriction.ActivityRestrictionCollection.IsEmpty())
				return default(T);
			return getter.Invoke(template.Restriction.ActivityRestrictionCollection.Single());
		}
	}
}