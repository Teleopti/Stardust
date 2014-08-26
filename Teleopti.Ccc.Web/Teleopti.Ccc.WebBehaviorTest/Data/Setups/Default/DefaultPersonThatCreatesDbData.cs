using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultPersonThatCreatesDbData : IHashableDataSetup
	{
		public static readonly IPerson PersonThatCreatesDbData =
			PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", DefaultPassword.ThePassword);

		public void Apply(IUnitOfWork uow)
		{
			var personRepository = new PersonRepository(uow);
			personRepository.Add(PersonThatCreatesDbData);
		}

		public int HashValue()
		{
			return (PersonThatCreatesDbData.ApplicationAuthenticationInfo.ApplicationLogOnName + 
							PersonThatCreatesDbData.ApplicationAuthenticationInfo.Password + 
							PersonThatCreatesDbData.PermissionInformation.Culture().DisplayName)
							.GetHashCode();
		}
	}
}