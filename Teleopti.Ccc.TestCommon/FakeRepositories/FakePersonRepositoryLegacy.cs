using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRepositoryLegacy2 : FakePersonRepository
	{
		public FakePersonRepositoryLegacy2() : base(null)
		{
		}

	}

	public class FakePersonRepositoryLegacy : FakePersonRepository
	{
		public FakePersonRepositoryLegacy() : base(null)
		{
			Has(PersonFactory.CreatePersonWithId());
		}

		public FakePersonRepositoryLegacy(IPerson person) : base(null)
		{
			Has(person);
		}

		public FakePersonRepositoryLegacy(params IPerson[] persons) : base(null)
		{
			persons.ForEach(Has);
		}
	}
}