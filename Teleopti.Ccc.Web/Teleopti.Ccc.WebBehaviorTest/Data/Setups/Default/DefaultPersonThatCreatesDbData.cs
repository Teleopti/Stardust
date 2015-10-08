using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultPersonThatCreatesDbData : IHashableDataSetup
	{
		public static IPerson PersonThatCreatesDbData =
			PersonFactory.CreatePerson("UserThatCreatesTestData");

		public void Apply(IUnitOfWork uow)
		{
			var personRepository = new PersonRepository(new ThisUnitOfWork(uow));
			personRepository.Add(PersonThatCreatesDbData);
		}

		public int HashValue()
		{
			return PersonThatCreatesDbData.Name.GetHashCode() ^
							PersonThatCreatesDbData.PermissionInformation.Culture().DisplayName.GetHashCode();
		}
	}
}