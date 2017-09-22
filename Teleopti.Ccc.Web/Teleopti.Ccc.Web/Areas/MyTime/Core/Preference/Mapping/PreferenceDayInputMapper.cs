using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayInputMapper
	{
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IDayOffTemplateRepository _dayOffRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IActivityRepository _activityRespository;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferenceDayInputMapper(IShiftCategoryRepository shiftCategoryRepository, IDayOffTemplateRepository dayOffRepository, IAbsenceRepository absenceRepository, IActivityRepository activityRespository, ILoggedOnUser loggedOnUser)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
			_dayOffRepository = dayOffRepository;
			_absenceRepository = absenceRepository;
			_activityRespository = activityRespository;
			_loggedOnUser = loggedOnUser;
		}

		public IPreferenceDay Map(PreferenceDayInput s, IPreferenceDay existingPreferenceDay = null)
		{
			if (existingPreferenceDay == null)
			{
				var person = _loggedOnUser.CurrentUser();
				var restriction = new PreferenceRestriction();
				map(s,restriction);
				return new PreferenceDay(person, s.Date, restriction) { TemplateName = s.TemplateName };
			}

			existingPreferenceDay.TemplateName = s.TemplateName;
			map(s, existingPreferenceDay.Restriction);
			return existingPreferenceDay;
		}

		private void map(PreferenceDayInput s, IPreferenceRestriction preferenceRestriction)
		{
			preferenceRestriction.ShiftCategory = s.PreferenceId != null
				? _shiftCategoryRepository.Get(s.PreferenceId.Value)
				: null;
			preferenceRestriction.Absence = s.PreferenceId != null ? _absenceRepository.Get(s.PreferenceId.Value) : null;
			preferenceRestriction.DayOffTemplate = s.PreferenceId != null ? _dayOffRepository.Get(s.PreferenceId.Value) : null;
			preferenceRestriction.StartTimeLimitation = new StartTimeLimitation(s.EarliestStartTime.ToTimeSpan(),
				s.LatestStartTime.ToTimeSpan());
			preferenceRestriction.EndTimeLimitation =
				new EndTimeLimitation(s.EarliestEndTime.ToTimeSpan(s.EarliestEndTimeNextDay),
					s.LatestEndTime.ToTimeSpan(s.LatestEndTimeNextDay));
				preferenceRestriction.WorkTimeLimitation = new WorkTimeLimitation(s.MinimumWorkTime, s.MaximumWorkTime);
				
					if (s.ActivityPreferenceId.HasValue)
					{
						if (preferenceRestriction.ActivityRestrictionCollection.Any())
						{
							var activityRestriction = preferenceRestriction.ActivityRestrictionCollection.Cast<ActivityRestriction>().Single();
							map(s, activityRestriction);
						}
						else
						{
							var activityRestriction = new ActivityRestriction();
							map(s, activityRestriction);
							preferenceRestriction.AddActivityRestriction(activityRestriction);
						}
					}
					else
					{
						var currentItems = new List<IActivityRestriction>(preferenceRestriction.ActivityRestrictionCollection);
						currentItems.ForEach(preferenceRestriction.RemoveActivityRestriction);
					}
		}

		private void map(PreferenceDayInput s, ActivityRestriction d)
		{
			d.Activity = _activityRespository.Get(s.ActivityPreferenceId.GetValueOrDefault());
			d.StartTimeLimitation = new StartTimeLimitation(s.ActivityEarliestStartTime.ToTimeSpan(),
				s.ActivityLatestStartTime.ToTimeSpan());
			d.EndTimeLimitation = new EndTimeLimitation(s.ActivityEarliestEndTime.ToTimeSpan(),
				s.ActivityLatestEndTime.ToTimeSpan());
			d.WorkTimeLimitation = new WorkTimeLimitation(s.ActivityMinimumTime, s.ActivityMaximumTime);
		}
	}
}