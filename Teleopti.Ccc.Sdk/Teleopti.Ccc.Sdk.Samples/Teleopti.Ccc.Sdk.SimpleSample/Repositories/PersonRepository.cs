using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class PersonRepository
    {
        private Dictionary<Guid, PersonDto> _personDictionary;

        public void Initialize(ITeleoptiOrganizationService teleoptiOrganizationService)
        {
            var people = teleoptiOrganizationService.GetPersons(false);
            _personDictionary = people.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public PersonDto GetById(Guid id)
        {
            PersonDto personDto;
            if (!_personDictionary.TryGetValue(id, out personDto))
            {
                personDto = new PersonDto();
            }
            return personDto;
        }
    }
}