using log4net;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultPersonThatCreatesData : IHashableDataSetup
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (DefaultPersonThatCreatesData));

		public static IPerson PersonThatCreatesDbData =
			PersonFactory.CreatePerson("UserThatCreatesTestData");

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var personRepository = new PersonRepository(currentUnitOfWork);
			personRepository.Add(PersonThatCreatesDbData);
		}

		public int HashValue()
		{
			log.Debug("PersonThatCreatesDbData.Name.GetHashCode() " + PersonThatCreatesDbData.Name.GetHashCode());
			log.Debug("PersonThatCreatesDbData.PermissionInformation.Culture().DisplayName.GetHashCode() " + PersonThatCreatesDbData.PermissionInformation.Culture().DisplayName.GetHashCode());
			return PersonThatCreatesDbData.Name.GetHashCode() ^
				   PersonThatCreatesDbData.PermissionInformation.Culture().DisplayName.GetHashCode();
		}
	}
}