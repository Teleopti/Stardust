using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultPersonThatCreatesData : IHashableDataSetup
	{
		public static IPerson PersonThatCreatesDbData = makePersonThatCreatesDbData();

		private static IPerson makePersonThatCreatesDbData()
		{
			var person = PersonFactory.CreatePerson("UserThatCreatesTestData");
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person.PermissionInformation.SetCulture(CultureInfoFactory.CreateSwedishCulture());
			person.PermissionInformation.SetUICulture(CultureInfoFactory.CreateSwedishCulture());
			return person;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var personRepository = new PersonRepository(currentUnitOfWork);
			personRepository.Add(PersonThatCreatesDbData);
		}

		public int HashValue()
		{
			return PersonThatCreatesDbData.Name.FirstName.GetHashCode() ^
				   PersonThatCreatesDbData.Name.LastName.GetHashCode() ^
				   PersonThatCreatesDbData.PermissionInformation.Culture().DisplayName.GetHashCode();
		}
	}
}