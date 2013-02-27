using System;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
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
				.ForMember(d => d.Preference, o => o.MapFrom(s =>
				{
					if (s.Restriction.DayOffTemplate != null)
						return s.Restriction.DayOffTemplate.Description.Name;
					if (s.Restriction.Absence != null)
						return s.Restriction.Absence.Description.Name;
					if (s.Restriction.ShiftCategory != null)
						return s.Restriction.ShiftCategory.Description.Name;
					return "";
				}))
				.ForMember(d => d.StartTimeLimitation, o => o.MapFrom(s => s.Restriction.StartTimeLimitation.StartTimeString.ToLower() + "-" + s.Restriction.StartTimeLimitation.EndTimeString.ToLower()))
				.ForMember(d => d.EndTimeLimitation, o => o.MapFrom(s => s.Restriction.EndTimeLimitation.StartTimeString.ToLower() + "-" + s.Restriction.EndTimeLimitation.EndTimeString.ToLower()))
				.ForMember(d => d.WorkTimeLimitation, o => o.MapFrom(s => s.Restriction.WorkTimeLimitation.StartTimeString + "-" + s.Restriction.WorkTimeLimitation.EndTimeString))
				.ForMember(d => d.Activity, o => o.MapFrom(s =>
				{
					var activityName = GetActivityRestrictionValue(s, r => r.Activity.Name);
					return string.IsNullOrEmpty(activityName) ? "(" + Resources.NoActivity + ")" : activityName;
				}))
				.ForMember(d => d.ActivityStartTimeLimitation, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.StartTimeLimitation.StartTimeString.ToLower() + "-" + r.StartTimeLimitation.EndTimeString.ToLower())))
				.ForMember(d => d.ActivityEndTimeLimitation, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.EndTimeLimitation.StartTimeString.ToLower() + "-" + r.EndTimeLimitation.EndTimeString.ToLower())))
				.ForMember(d => d.ActivityTimeLimitation, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.WorkTimeLimitation.StartTimeString + "-" + r.WorkTimeLimitation.EndTimeString)))
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