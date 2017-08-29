using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonRepositoryLegacy2 : FakePersonRepository
	{
		public FakePersonRepositoryLegacy2() : base(new FakeStorage())
		{
		}

	}

	public class FakePersonRepositoryLegacy : FakePersonRepository
	{
		public FakePersonRepositoryLegacy() : base(new FakeStorage())
		{
			Has(PersonFactory.CreatePersonWithId());
		}

		public FakePersonRepositoryLegacy(IPerson person) : base(new FakeStorage())
		{
			Has(person);
		}

		public FakePersonRepositoryLegacy(params IPerson[] persons) : base(new FakeStorage())
		{
			persons.ForEach(Has);
		}
	}
}