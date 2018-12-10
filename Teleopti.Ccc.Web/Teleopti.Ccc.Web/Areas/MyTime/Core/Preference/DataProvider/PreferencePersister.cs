using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePersister : IPreferencePersister
	{
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IMustHaveRestrictionSetter _mustHaveRestrictionSetter;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly PreferenceDayInputMapper _inputMapper;
		private readonly PreferenceDayViewModelMapper _mapper;

		public PreferencePersister(
			IPreferenceDayRepository preferenceDayRepository, 
			IMustHaveRestrictionSetter mustHaveRestrictionSetter,
			ILoggedOnUser loggedOnUser, PreferenceDayInputMapper inputMapper, PreferenceDayViewModelMapper mapper)
		{
			_preferenceDayRepository = preferenceDayRepository;
			_mustHaveRestrictionSetter = mustHaveRestrictionSetter;
			_loggedOnUser = loggedOnUser;
			_inputMapper = inputMapper;
			_mapper = mapper;
		}

		public PreferenceDayViewModel Persist(PreferenceDayInput input)
		{
			var preferenceDays = _preferenceDayRepository.Find(input.Date, _loggedOnUser.CurrentUser());
			preferenceDays = deleteOrphanPreferenceDays(preferenceDays);
			var preferenceDay = preferenceDays.SingleOrDefaultNullSafe();
			if (preferenceDay == null)
			{
				preferenceDay = _inputMapper.Map(input);
				_preferenceDayRepository.Add(preferenceDay);
			}
			else
			{
				clearExtendedAndMustHave(preferenceDay);
				_inputMapper.Map(input, preferenceDay);
			}

			return _mapper.Map(preferenceDay);
		}

		public IDictionary<DateOnly, PreferenceDayViewModel> PersistMultiDays(MultiPreferenceDaysInput input)
		{
			var ret = new Dictionary<DateOnly, PreferenceDayViewModel>();
			if (input?.Dates == null || !input.Dates.Any())
			{
				return ret;
			}

			var uniqueDates = input.Dates.Distinct();
			foreach (var date in uniqueDates)
			{
				var dayInput = new PreferenceDayInput
				{
					PreferenceId = input.PreferenceId,
					TemplateName = input.TemplateName,
					Date = date,
					ActivityEarliestEndTime = input.ActivityEarliestEndTime,
					ActivityEarliestStartTime = input.ActivityEarliestStartTime,
					ActivityLatestEndTime = input.ActivityLatestEndTime,
					ActivityLatestStartTime = input.ActivityLatestStartTime,
					ActivityMaximumTime = input.ActivityMaximumTime,
					ActivityMinimumTime = input.ActivityMinimumTime,
					ActivityPreferenceId = input.ActivityPreferenceId,
					EarliestEndTime = input.EarliestEndTime,
					EarliestStartTime = input.EarliestStartTime,
					EarliestEndTimeNextDay = input.EarliestEndTimeNextDay,
					LatestStartTime = input.LatestStartTime,
					LatestEndTime = input.LatestEndTime,
					LatestEndTimeNextDay = input.LatestEndTimeNextDay,
					MinimumWorkTime = input.MinimumWorkTime,
					MaximumWorkTime = input.MaximumWorkTime
				};
				var preferenceVm = Persist(dayInput);
				ret.Add(date, preferenceVm);
			}

			return ret;
		}

		public bool MustHave(MustHaveInput input)
		{
			return _mustHaveRestrictionSetter.SetMustHave(input.Date, _loggedOnUser.CurrentUser(), input.MustHave);
		}

		private static void clearExtendedAndMustHave(IPreferenceDay preferenceDay)
		{
			if (preferenceDay.Restriction == null) { return; }

			preferenceDay.Restriction.StartTimeLimitation = new StartTimeLimitation();
			preferenceDay.Restriction.EndTimeLimitation = new EndTimeLimitation();
			preferenceDay.Restriction.WorkTimeLimitation = new WorkTimeLimitation();
			preferenceDay.Restriction.MustHave = false;
			preferenceDay.TemplateName = null;
		}

		private IList<IPreferenceDay> deleteOrphanPreferenceDays(IList<IPreferenceDay> preferenceDays)
		{
			preferenceDays = preferenceDays?.OrderBy(k => k.UpdatedOn).ToList() ?? new List<IPreferenceDay>();
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

	    public IDictionary<DateOnly, PreferenceDayViewModel> Delete(List<DateOnly> dates)
	    {
	        var preferenceDayViewModel = new Dictionary<DateOnly, PreferenceDayViewModel>();
	        foreach (var date in dates)
	        {
	            var preferences = _preferenceDayRepository.FindAndLock(date, _loggedOnUser.CurrentUser());
	            preferenceDayViewModel.Add(date, Delete(preferences));

	        }
	        return preferenceDayViewModel;
	    }
	}
}