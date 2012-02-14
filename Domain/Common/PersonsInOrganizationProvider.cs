using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonsInOrganizationProvider 
        : PersonProvider
    {
        private readonly DateTimePeriod _dateTimePeriod;

        public PersonsInOrganizationProvider(IEnumerable<IPerson> persons) 
            : base(persons)
        {}

        public PersonsInOrganizationProvider(IPersonRepository personRepository, DateTimePeriod dateTimePeriod)
            :base(personRepository)
        {
            _dateTimePeriod = dateTimePeriod;
        }

        public override IList<IPerson> GetPersons()
        {
            if(PersonRepository!=null)
                SetInnerPersons(PersonRepository.FindPeopleInOrganization(_dateTimePeriod, true));
            return InnerPersons;
        }
    }
}