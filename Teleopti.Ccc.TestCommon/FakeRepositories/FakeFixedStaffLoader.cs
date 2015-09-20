using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeFixedStaffLoader : IFixedStaffLoader
	{
		private IPerson[] _people;

		public void SetPeople(params IPerson[] people)
		{
			_people = people;
		}

		public PeopleSelection Load(DateOnlyPeriod period)
		{
			return new PeopleSelection(_people,_people);
		}
	}
}