using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceTemplatePersister : IPreferenceTemplatePersister
	{
		private readonly IExtendedPreferenceTemplateRepository _preferenceTemplateRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IDayOffTemplateRepository _dayOffRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ExtendedPreferenceTemplateMapper _extendedPreferenceTemplateMapper;

		public PreferenceTemplatePersister(IExtendedPreferenceTemplateRepository preferenceTemplateRepository, ILoggedOnUser loggedOnUser, IShiftCategoryRepository shiftCategoryRepository, IAbsenceRepository absenceRepository, IDayOffTemplateRepository dayOffRepository, IActivityRepository activityRepository)
		{
			_preferenceTemplateRepository = preferenceTemplateRepository;
			_loggedOnUser = loggedOnUser;
			_shiftCategoryRepository = shiftCategoryRepository;
			_absenceRepository = absenceRepository;
			_dayOffRepository = dayOffRepository;
			_activityRepository = activityRepository;
			_extendedPreferenceTemplateMapper = new ExtendedPreferenceTemplateMapper();
		}

		public PreferenceTemplateViewModel Persist(PreferenceTemplateInput input)
		{
			var template = new PreferenceRestrictionTemplate
				{
					ShiftCategory = input.PreferenceId.HasValue ? _shiftCategoryRepository.Get(input.PreferenceId.Value) : null,
					Absence = input.PreferenceId.HasValue ? _absenceRepository.Get(input.PreferenceId.Value) : null,
				DayOffTemplate = input.PreferenceId.HasValue ? _dayOffRepository.Get(input.PreferenceId.Value) : null,
				StartTimeLimitation = new StartTimeLimitation(input.EarliestStartTime.ToTimeSpan(), input.LatestStartTime.ToTimeSpan()),
				EndTimeLimitation = new EndTimeLimitation(input.EarliestEndTime.ToTimeSpan(input.EarliestEndTimeNextDay), input.LatestEndTime.ToTimeSpan(input.LatestEndTimeNextDay)),
				WorkTimeLimitation = new WorkTimeLimitation(input.MinimumWorkTime, input.MaximumWorkTime),
			};
			if (input.ActivityPreferenceId.HasValue)
			{
				var activityRestriction = new ActivityRestrictionTemplate(_activityRepository.Get(input.ActivityPreferenceId.Value))
				{
					StartTimeLimitation = new StartTimeLimitation(input.ActivityEarliestStartTime.ToTimeSpan(),
						input.ActivityLatestStartTime.ToTimeSpan()),
					EndTimeLimitation = new EndTimeLimitation(input.ActivityEarliestEndTime.ToTimeSpan(),
						input.ActivityLatestEndTime.ToTimeSpan()),
					WorkTimeLimitation = new WorkTimeLimitation(input.ActivityMinimumTime, input.ActivityMaximumTime)
				};

				template.AddActivityRestriction(activityRestriction);
			}
			else
			{
				var currentItems = new List<IActivityRestriction>(template.ActivityRestrictionCollection);
				currentItems.ForEach(template.RemoveActivityRestriction);
			}

			var extendedPreferenceTemplate = new ExtendedPreferenceTemplate(_loggedOnUser.CurrentUser(), template, input.NewTemplateName, Color.White);

			_preferenceTemplateRepository.Add(extendedPreferenceTemplate);
			return _extendedPreferenceTemplateMapper.Map(extendedPreferenceTemplate);
		}
		
		public void Delete(Guid templateId)
		{
			var template = _preferenceTemplateRepository.Find(templateId);
			if (template == null)
				throw new HttpException(404, "Preference template not found");
			_preferenceTemplateRepository.Remove(template);
		}
	}
}