using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class PersonRepositoryPossibleShiftTradeTest : DatabaseTest
	{
		private IPersonRepository target;
		private IPerson myself;

		protected override void SetupForRepositoryTest()
		{
			target = new PersonRepository(UnitOfWork);
			myself = new Person();
			addValidPersonStuff(myself);
			PersistAndRemoveFromUnitOfWork(myself);
		}

		[Test]
		public void ShouldNotFetchMyself()
		{
			target.FindPossibleShiftTrades(myself)
				.Should().Not.Contain(myself);
		}

		[Test]
		public void ShouldFetchValidPerson()
		{
			var valid = new Person();
			addValidPersonStuff(valid);
			PersistAndRemoveFromUnitOfWork(valid);

			target.FindPossibleShiftTrades(myself)
				.Should().Contain(valid);
		}

		private static void addValidPersonStuff(IPerson person)
		{
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
		}

	}
}