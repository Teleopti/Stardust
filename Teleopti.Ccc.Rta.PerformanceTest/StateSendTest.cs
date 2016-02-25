using System;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendStateTest")]
	public class StateSendTest
	{
		[Test]
		public void MeasurePerformance()
		{
			var dataFactory = new DataFactory(GlobalUnitOfWorkState.UnitOfWorkAction);
			dataFactory.Apply(new PersonSetup {Name = "roger"});
			Thread.Sleep(TimeSpan.FromMinutes(new Random().Next(1, 2)));
		}
	}

	public class PersonSetup : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var personRepository = new PersonRepository(currentUnitOfWork);
			var person = new Person {Name = new Name(Name, "")};
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());
			personRepository.Add(person);
		}

	}
}