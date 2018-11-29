using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class FixedStaffLoader : IFixedStaffLoader
	{
		private readonly IPersonRepository _personRepository;

		public FixedStaffLoader(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public PeopleSelection Load(DateOnlyPeriod period)
		{
			var allPeople = _personRepository.FindAllAgentsLight(period).ToList();
			var peopleToSchedule = allPeople.FixedStaffPeople(period);

			return new PeopleSelection(allPeople, peopleToSchedule.ToList());
		}
	}
}