using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class PersonsInOrganizationProvider 
        : PersonProvider
    {
    	private readonly DateOnlyPeriod _period;

        public PersonsInOrganizationProvider(IEnumerable<IPerson> persons) 
            : base(persons)
        {}

        public PersonsInOrganizationProvider(IPersonRepository personRepository, DateOnlyPeriod period)
            :base(personRepository)
        {
        	_period = period;
        }

    	public override IList<IPerson> GetPersons()
        {
            if(PersonRepository!=null)
                SetInnerPersons(PersonRepository.FindPeopleInOrganization(_period, true));
            return InnerPersons;
        }
    }
}