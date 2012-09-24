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

		/// <summary>
		/// Tries the toggle must have.
		/// </summary>
		/// <param name="selectedDay">The selected day.</param>
		/// <param name="mustHaveSet">if set to <c>true</c> [must have set].</param>
		/// <param name="schedulePeriod">The schedule period.</param>
		/// <returns><c>True</c> if the persist process can be continued, <c>True</c> if the persist should be stopped</returns>
		public bool TryToggleMustHave(DateOnly selectedDay, bool mustHaveSet, DateOnlyPeriod schedulePeriod)
		{
			IPreferenceDay preferenceDay;
			if (mustHaveSet)
			{
				var preferenceDays = _preferenceDayRepository.Find(schedulePeriod, new [] {_loggedOnUser.CurrentUser()});
				var nbrOfDaysWithMustHave = preferenceDays.Count(p => p.Restriction.MustHave);
				var currentSchedulePeriod = _loggedOnUser.CurrentUser().SchedulePeriod(selectedDay);
				if (nbrOfDaysWithMustHave >= currentSchedulePeriod.MustHavePreference)
				{
					return false;
				}
				preferenceDay = preferenceDays.SingleOrDefault(d => d.RestrictionDate == selectedDay);
			}
			else
			{
				var preferenceDays = _preferenceDayRepository.Find(selectedDay, _loggedOnUser.CurrentUser());
				preferenceDay = preferenceDays.SingleOrDefaultNullSafe();
			}
			if (preferenceDay != null)
			{
				preferenceDay.Restriction.MustHave = mustHaveSet;
			}

			return true;		}
		}

		public PreferenceDayViewModel MustHave(MustHaveInput input)
		{
			var preferenceDays = _preferenceDayRepository.Find(input.Date, _loggedOnUser.CurrentUser());
			preferenceDays = DeleteOrphanPreferenceDays(preferenceDays);
			var preferenceDay = preferenceDays.SingleOrDefaultNullSafe();
			if (preferenceDay != null)
			{
				preferenceDay.Restriction.MustHave = input.MustHave;
			}

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