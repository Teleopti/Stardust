using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	public class PersonSetup : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var personRepository = new PersonRepository(currentUnitOfWork);
			var person = new Person {Name = new Name(Name, "")};
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());

			var personPeriod = new PersonPeriodConfigurable
			{
				ExternalLogon = Name
			};
			personPeriod.Apply(currentUnitOfWork.Current(), person, CultureInfoFactory.CreateEnglishCulture());

			personRepository.Add(person);	
		}

	}
}