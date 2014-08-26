using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultPersonThatCreatesDbData : IDataSetup
	{
		public static readonly IPerson PersonThatCreatesDbData =
			PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", DefaultPassword.ThePassword);

		public void Apply(IUnitOfWork uow)
		{
			var personRepository = new PersonRepository(uow);
			personRepository.Add(PersonThatCreatesDbData);
		}
	}
}