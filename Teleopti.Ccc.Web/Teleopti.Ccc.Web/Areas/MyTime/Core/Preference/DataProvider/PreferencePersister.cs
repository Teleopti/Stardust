using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePersister : IPreferencePersister
	{
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IMappingEngine _mapper;
		private readonly IMustHaveRestrictionSetter _mustHaveRestrictionSetter;
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferencePersister(
			IPreferenceDayRepository preferenceDayRepository, 
			IMappingEngine mapper,  
			IMustHaveRestrictionSetter mustHaveRestrictionSetter,
			ILoggedOnUser loggedOnUser)
		{
			_preferenceDayRepository = preferenceDayRepository;
			_mapper = mapper;
			_mustHaveRestrictionSetter = mustHaveRestrictionSetter;
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
				ClearExtendedAndMustHave(preferenceDay);
				_mapper.Map(input, preferenceDay);

			}
			return _mapper.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay);
		}

		public bool MustHave(MustHaveInput input)
		{
			return _mustHaveRestrictionSetter.SetMustHave(input.Date, _loggedOnUser.CurrentUser(), input.MustHave);
		}

		private static void ClearExtendedAndMustHave(IPreferenceDay preferenceDay)
		{
			if (preferenceDay.Restriction != null)
			{
				preferenceDay.Restriction.StartTimeLimitation = new StartTimeLimitation();
				preferenceDay.Restriction.EndTimeLimitation = new EndTimeLimitation();
				preferenceDay.Restriction.WorkTimeLimitation = new WorkTimeLimitation();
				preferenceDay.Restriction.MustHave = false;
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

		public PreferenceDayViewModel Delete(IList<IPreferenceDay> preferences)
		{
			foreach (var preferenceDay in preferences)
			{
				_preferenceDayRepository.Remove(preferenceDay);
			}
			return new PreferenceDayViewModel { Color = "" };
		}


		public IEnumerable<PreferenceDayViewModel> Delete(List<DateOnly> dates)
		{
			return dates.Select (date => _preferenceDayRepository.FindAndLock (date, _loggedOnUser.CurrentUser()))
				.Where (preferences => !preferences.IsEmpty())
				.Select (Delete).ToList();
		}
	}
}