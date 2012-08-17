using System;
using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayViewModelMappingProfile : Profile
	{
		private readonly IResolve<IExtendedPreferencePredicate> _extendedPreferencePredicate;

		public PreferenceDayViewModelMappingProfile(IResolve<IExtendedPreferencePredicate> extendedPreferencePredicate)
		{
			_extendedPreferencePredicate = extendedPreferencePredicate;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IPreferenceDay, PreferenceDayViewModel>()
				.ForMember(d => d.Preference, o => o.MapFrom(s =>
				                                             	{
				                                             		if (s.Restriction.DayOffTemplate != null)
				                                             			return s.Restriction.DayOffTemplate.Description.Name;
				                                             		if (s.Restriction.Absence != null)
				                                             			return s.Restriction.Absence.Description.Name;
				                                             		if (s.Restriction.ShiftCategory != null)
				                                             			return s.Restriction.ShiftCategory.Description.Name;
				                                             		if (_extendedPreferencePredicate.Invoke().IsExtended(s))
				                                             			return Resources.Extended;
				                                             		return null;
				                                             	}))
				.ForMember(d => d.Color, o => o.MapFrom(s =>
				                                        	{
				                                        		if (s.Restriction.DayOffTemplate != null)
				                                        			return s.Restriction.DayOffTemplate.DisplayColor.ToHtml();
				                                        		if (s.Restriction.Absence != null)
				                                        			return s.Restriction.Absence.DisplayColor.ToHtml();
				                                        		if (s.Restriction.ShiftCategory != null)
				                                        			return s.Restriction.ShiftCategory.DisplayColor.ToHtml();
				                                        		return "";
				                                        	}))
				.ForMember(d => d.Extended, o => o.MapFrom(s => _extendedPreferencePredicate.Invoke().IsExtended(s)))
				.ForMember(d => d.ExtendedTitle, o => o.MapFrom(s =>
				                                                	{
				                                                		if (s.Restriction.DayOffTemplate != null)
				                                                			return s.Restriction.DayOffTemplate.Description.Name;
				                                                		if (s.Restriction.Absence != null)
				                                                			return s.Restriction.Absence.Description.Name;
				                                                		if (s.Restriction.ShiftCategory != null)
				                                                			return s.Restriction.ShiftCategory.Description.Name;
				                                                		return Resources.Extended;
				                                                	}))
				.ForMember(d => d.StartTimeLimitation, o => o.MapFrom(s => s.Restriction.StartTimeLimitation.StartTimeString + "-" + s.Restriction.StartTimeLimitation.EndTimeString))
				.ForMember(d => d.EndTimeLimitation, o => o.MapFrom(s => s.Restriction.EndTimeLimitation.StartTimeString + "-" + s.Restriction.EndTimeLimitation.EndTimeString))
				.ForMember(d => d.WorkTimeLimitation, o => o.MapFrom(s => s.Restriction.WorkTimeLimitation.StartTimeString + "-" + s.Restriction.WorkTimeLimitation.EndTimeString))
				.ForMember(d => d.Activity, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.Activity.Name)))
				.ForMember(d => d.ActivityStartTimeLimitation, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.StartTimeLimitation.StartTimeString + "-" + r.StartTimeLimitation.EndTimeString)))
				.ForMember(d => d.ActivityEndTimeLimitation, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.EndTimeLimitation.StartTimeString + "-" + r.EndTimeLimitation.EndTimeString)))
				.ForMember(d => d.ActivityTimeLimitation, o => o.MapFrom(s => GetActivityRestrictionValue(s, r => r.WorkTimeLimitation.StartTimeString + "-" + r.WorkTimeLimitation.EndTimeString)))
				;

		}

		private T GetActivityRestrictionValue<T>(IPreferenceDay preferenceDay, Func<IActivityRestriction, T> getter)
		{
			if (preferenceDay.Restriction.ActivityRestrictionCollection.IsEmpty())
				return default(T);
			return getter.Invoke(preferenceDay.Restriction.ActivityRestrictionCollection.Single());
		}
	}

}