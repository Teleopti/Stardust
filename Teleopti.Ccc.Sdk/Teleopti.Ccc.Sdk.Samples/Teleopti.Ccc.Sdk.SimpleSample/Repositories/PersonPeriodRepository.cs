using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.SimpleSample.ViewModel;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class PersonPeriodRepository
    {
    	private readonly ITeleoptiOrganizationService _teleoptiOrganizationService;
    	private Dictionary<Guid, IEnumerable<PersonPeriodDetailDto>> _personPeriodDictionary;
		private Dictionary<GuidDateKey, PersonSkillPeriodDto>  _personSkillPeriodDictionary;

    	public PersonPeriodRepository(ITeleoptiOrganizationService teleoptiOrganizationService)
    	{
    		_teleoptiOrganizationService = teleoptiOrganizationService;
    	}

    	public void Initialize(DateOnlyDto startDate,DateOnlyDto endDate)
        {
            var periods = _teleoptiOrganizationService.GetPersonPeriods(new PersonPeriodLoadOptionDto {LoadAll = true }, startDate, endDate);
            _personPeriodDictionary = periods.GroupBy(pp => pp.PersonId).ToDictionary(k => k.Key, v => v.Select(p => p));

    		var skillPeriods =
    			_teleoptiOrganizationService.GetPersonSkillPeriodsForPersons(
    				_personPeriodDictionary.Keys.Select(p => new PersonDto {Id = p}).ToArray(), startDate, endDate);
    		_personSkillPeriodDictionary =
    			skillPeriods.ToDictionary(k => new GuidDateKey {Id = k.PersonId, Date = k.DateFrom.DateTime}, v => v);
        }

		public IEnumerable<PersonDetailContainer> GetPersonPeriods()
		{
			foreach (var item in _personPeriodDictionary.Values.SelectMany(v => v))
			{
				var detail = new PersonDetailContainer {PersonPeriod = item};
				PersonSkillPeriodDto foundSkillPeriod;
				if (_personSkillPeriodDictionary.TryGetValue(new GuidDateKey {Date = item.StartDate.DateTime, Id = item.PersonId}, out foundSkillPeriod))
				{
					detail.PersonSkillPeriod = foundSkillPeriod;
				}
				yield return detail;
			}
		}
    }
}