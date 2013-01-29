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
			myself = createValidPerson();
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
			var valid = createValidPerson();
			PersistAndRemoveFromUnitOfWork(valid);

			target.FindPossibleShiftTrades(myself)
				.Should().Contain(valid);
		}

		[Test]
		public void ShouldFetchInRandomOrder()
		{
			const int noOfPersons = 10;
			for (var i = 0; i < noOfPersons; i++)
			{
				PersistAndRemoveFromUnitOfWork(createValidPerson());
			}

			var firstFetch = target.FindPossibleShiftTrades(myself);

			const int retries = 5;
			for (var i = 0; i < retries; i++)
			{
				var possible = target.FindPossibleShiftTrades(myself);
				if (possible.PositionOfFirstDifference(firstFetch) != -1)
					return;
			}
			Assert.Fail("No randomness of possible shift trades");
		}

		private static IPerson createValidPerson()
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			return person;
		}

	}
}