using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRepositoryLegacy : FakePersonRepository
	{
		public FakePersonRepositoryLegacy()
		{
			Has(PersonFactory.CreatePersonWithId());
		}

		public FakePersonRepositoryLegacy(IPerson person)
		{
			Has(person);
		}

		public FakePersonRepositoryLegacy(params IPerson[] persons)
		{
			persons.ForEach(Has);
		}
	}
}