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
			_people = _people ?? new IPerson[0];
			return new PeopleSelection(_people, _people);
		}
	}
}