using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePersister : IPreferencePersister
	{
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IMappingEngine _mapper;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferencePersister(IPreferenceDayRepository preferenceDayRepository, IMappingEngine mapper, ILoggedOnUser loggedOnUser)
		{
			_preferenceDayRepository = preferenceDayRepository;
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
		}

		public PreferenceDayViewModel Persist(PreferenceDayInput input)
		{
			var preferenceDays = _preferenceDayRepository.Find(input.Date, _loggedOnUser.CurrentUser());
			preferenceDays = DeleteOrphanPreferenceDays(preferenceDays);
			var preferenceDay = preferenceDays.SingleOrDefaultNullSafe();
			if (preferenceDay == null)
			{
				preferenceDay = _mapper.Map<PreferenceDayInput, IPreferenceDay>(input);
				_preferenceDayRepository.Add(preferenceDay);
			}
			else
			{
					ClearExtendedData(preferenceDay);
				_mapper.Map(input, preferenceDay);

			}
			return _mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);
		}

		public PreferenceDayViewModel MustHave(DateOnlyPeriod schedulePeriod, MustHaveInput input)
		{
			var preferenceDay = TryToggleMustHave(schedulePeriod, input);
			return _mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);
		}

		private static void ClearExtendedData(IPreferenceDay preferenceDay)
		{
			if (preferenceDay.Restriction != null)
			{
				preferenceDay.Restriction.StartTimeLimitation = new StartTimeLimitation();
				preferenceDay.Restriction.EndTimeLimitation = new EndTimeLimitation();
				preferenceDay.Restriction.WorkTimeLimitation = new WorkTimeLimitation();
				preferenceDay.TemplateName = null;
			}
		}

		/// <summary>
		/// Tries to toggle the must have property if it is found or is NOT over the limit.
		/// </summary>
		/// <param name="schedulePeriod">The schedule period.</param>
		/// <param name="input">The input.</param>
		private IPreferenceDay TryToggleMustHave(DateOnlyPeriod schedulePeriod, MustHaveInput input)
		{
			var mustHave = input.MustHave;
			var selectedDay = input.Date;
			IPreferenceDay preferenceDay;
			if (mustHave)
			{
				var preferenceDays = _preferenceDayRepository.Find(schedulePeriod, new[] { _loggedOnUser.CurrentUser() });
				preferenceDays = DeleteOrphanPreferenceDays(preferenceDays);
				var nbrOfDaysWithMustHave = preferenceDays.Count(p => p.Restriction.MustHave);
				var currentSchedulePeriod = _loggedOnUser.CurrentUser().SchedulePeriod(selectedDay);
				if (nbrOfDaysWithMustHave >= currentSchedulePeriod.MustHavePreference)
					return null;
				preferenceDay = preferenceDays.SingleOrDefault(d => d.RestrictionDate == selectedDay);
			}
			else
			{
				var preferenceDays = _preferenceDayRepository.Find(selectedDay, _loggedOnUser.CurrentUser());
				preferenceDays = DeleteOrphanPreferenceDays(preferenceDays);
				preferenceDay = preferenceDays.SingleOrDefaultNullSafe();
			}

			if (preferenceDay != null)
			{
				preferenceDay.Restriction.MustHave = mustHave;
			}

			return preferenceDay;
		}
		private IList<IPreferenceDay> DeleteOrphanPreferenceDays(IList<IPreferenceDay> preferenceDays)
		{
			preferenceDays = preferenceDays != null
			                 	? preferenceDays.OrderBy(k => k.UpdatedOn).ToList()
			                 	: new List<IPreferenceDay>();
			while (preferenceDays.Count > 1)
			{
				_preferenceDayRepository.Remove(preferenceDays.First());
				preferenceDays.Remove(preferenceDays.First());
			}
			return preferenceDays;
		}

		public PreferenceDayViewModel Delete(DateOnly date)
		{
			var preferences = _preferenceDayRepository.Find(date, _loggedOnUser.CurrentUser());
			if (preferences.IsEmpty())
				throw new HttpException(404, "Preference not found");

			foreach (var preferenceDay in preferences)
			{
				_preferenceDayRepository.Remove(preferenceDay);
			}
			return new PreferenceDayViewModel { Color = ""};
		}

	}
}