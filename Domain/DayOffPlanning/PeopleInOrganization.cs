using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public class PeopleInOrganization : IPeopleInOrganization
	{
		private readonly IPersonRepository _personRepository;

		public PeopleInOrganization(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			return _personRepository.FindPeopleInOrganization(period, false);
		}
	}
}