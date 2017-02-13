using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAgentGroupStaffLoader : IAgentGroupStaffLoader
	{
		private IPerson[] _people;

		public void SetPeople(params IPerson[] people)
		{
			_people = people;
		}

		public PeopleSelection Load(DateOnlyPeriod period, IAgentGroup agentGroup)
		{
			_people = _people ?? new IPerson[0];
			return new PeopleSelection(_people,_people);
		}
	}
}