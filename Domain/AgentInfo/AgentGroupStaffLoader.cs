using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class AgentGroupStaffLoader : IAgentGroupStaffLoader
	{
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly IPersonRepository _personRepository;


		public AgentGroupStaffLoader(IFixedStaffLoader fixedStaffLoader, IPersonRepository personRepository)
		{
			_fixedStaffLoader = fixedStaffLoader;
			_personRepository = personRepository;
		}

		public PeopleSelection Load(DateOnlyPeriod period, IAgentGroup agentGroup)
		{
			if (agentGroup == null)
			{
				return _fixedStaffLoader.Load(period);
			}
			var result = _personRepository.FindPeopleInAgentGroup(agentGroup, period);
			var peopleToSchedule = result.FixedStaffPeople(period);
			return new PeopleSelection(result, peopleToSchedule);
		}
	}
}