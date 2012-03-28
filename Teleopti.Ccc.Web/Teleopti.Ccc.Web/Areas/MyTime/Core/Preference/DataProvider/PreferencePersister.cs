using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
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

		public PreferenceDayInputResult Persist(PreferenceDayInput input)
		{
			var preferenceDay = _preferenceDayRepository.Find(input.Date, _loggedOnUser.CurrentUser()).SingleOrDefaultNullSafe();
			if (preferenceDay == null)
			{
				preferenceDay = _mapper.Map<PreferenceDayInput, IPreferenceDay>(input);
				_preferenceDayRepository.Add(preferenceDay);
			}
			else
			{
				_mapper.Map(input, preferenceDay);
			}
			return _mapper.Map<IPreferenceDay, PreferenceDayInputResult>(preferenceDay);
		}

		public PreferenceDayInputResult Delete(DateOnly date)
		{
			var preferences = _preferenceDayRepository.Find(date, _loggedOnUser.CurrentUser());
			if (preferences.IsEmpty())
				throw new HttpException(404, "Preference not found");

			_preferenceDayRepository.Remove(preferences.Single());
			return new PreferenceDayInputResult { Date = date.ToFixedClientDateOnlyFormat(), HexColor = ""};
		}
	}
}