using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class PersonPeriodRepository
    {
        private Dictionary<Guid, IEnumerable<PersonPeriodDetailDto>> _personPeriodDictionary;

        public void Initialize(ITeleoptiOrganizationService teleoptiOrganizationService, DateOnlyDto startDate,DateOnlyDto endDate)
        {
            var periods = teleoptiOrganizationService.GetPersonPeriods(new PersonPeriodLoadOptionDto { LoadAll = true }, startDate, endDate);
            _personPeriodDictionary = periods.GroupBy(pp => pp.PersonId).ToDictionary(k => k.Key, v => v.Select(p => p));
        }

        public IEnumerable<PersonPeriodDetailDto> GetByPersonId(Guid personId)
        {
            IEnumerable<PersonPeriodDetailDto> foundPersonPeriodDetails;
            if (!_personPeriodDictionary.TryGetValue(personId,out foundPersonPeriodDetails))
            {
                foundPersonPeriodDetails = new List<PersonPeriodDetailDto>();
            }
            return foundPersonPeriodDetails;
        }

        public IEnumerable<PersonPeriodDetailDto> AllPersonPeriodDetails()
        {
            return from pp in _personPeriodDictionary.Values
                   from p in pp
                   select p;
        }
    }
 }