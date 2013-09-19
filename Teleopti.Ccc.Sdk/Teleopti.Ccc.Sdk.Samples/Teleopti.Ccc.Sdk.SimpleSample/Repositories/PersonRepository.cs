using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class PersonRepository
    {
        private readonly ITeleoptiOrganizationService _teleoptiOrganizationService;
        private Dictionary<Guid, PersonDto> _personDictionary;

        public PersonRepository(ITeleoptiOrganizationService teleoptiOrganizationService)
        {
            _teleoptiOrganizationService = teleoptiOrganizationService;
        }

        public void Initialize()
        {
            var people = _teleoptiOrganizationService.GetPersons(false);
            _personDictionary = people.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public void TerminatePerson(PersonDto personDto)
        {
            var dateOnlyDto = new DateOnlyDto(2013, 10, 31);
            personDto.TerminationDate = dateOnlyDto;
            _teleoptiOrganizationService.UpdatePerson(personDto);
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