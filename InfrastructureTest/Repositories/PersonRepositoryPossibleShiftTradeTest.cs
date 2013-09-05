using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class PersonRepositoryPossibleShiftTradeTest : DatabaseTest
	{
		private IPersonRepository target;
		private IPerson myself;
		private ISite mySite;
		private ITeam myTeam;
		private IWorkflowControlSet wcs;
		private IPersonPeriod personPeriodMyTeam;

		protected override void SetupForRepositoryTest()
		{
			target = new PersonRepository(UnitOfWork);
			wcs = new WorkflowControlSet("hej");
			mySite = new Site("My Site");
			myTeam = new Team{Site = mySite, Description = new Description("My Team")};
			personPeriodMyTeam = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, myTeam);
			
			myself = createPersonInMyTeam();

			PersistAndRemoveFromUnitOfWork(wcs);
			PersistAndRemoveFromUnitOfWork(mySite);
			PersistAndRemoveFromUnitOfWork(myTeam);
			PersistAndRemoveFromUnitOfWork(personPeriodMyTeam.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personPeriodMyTeam.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriodMyTeam.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(myself);
		}

		[Test]
		public void ShouldNotFetchMyself()
		{
			target.FindPossibleShiftTrades(myself, false, DateOnly.Today)
				.Should().Not.Contain(myself);
		}

		[Test]
		public void ShouldNotFetchPersonWithNoWorkflowControlSet()
		{
			var p = createPersonInMyTeam();
			p.WorkflowControlSet = null;
			PersistAndRemoveFromUnitOfWork(p);

			target.FindPossibleShiftTrades(myself, false, DateOnly.Today)
				.Should().Not.Contain(p);
		}

		[Test]
		public void ShouldFetchValidPerson()
		{
			var valid = createPersonInMyTeam();
			PersistAndRemoveFromUnitOfWork(valid);

			target.FindPossibleShiftTrades(myself, false, DateOnly.Today)
				.Should().Contain(valid);
		}

		[Test]
		public void ShouldFetchInRandomOrder()
		{
			const int noOfPersons = 10;
			for (var i = 0; i < noOfPersons; i++)
			{
				PersistAndRemoveFromUnitOfWork(createPersonInMyTeam());
			}

			var firstFetch = target.FindPossibleShiftTrades(myself, false, DateOnly.Today);

			const int retries = 5;
			for (var i = 0; i < retries; i++)
			{
				var possible = target.FindPossibleShiftTrades(myself, false, DateOnly.Today);
				if (possible.PositionOfFirstDifference(firstFetch) != -1)
					return;
			}
			Assert.Fail("No randomness of possible shift trades");
		}

		[Test]
		public void ShouldFetchOnlyPersonsFromMyTeam()
		{
			var myTeamPerson = createPersonInMyTeam();
			var otherTeamPerson = createPersonInOtherTeam();

			PersistAndRemoveFromUnitOfWork(myTeamPerson);
			PersistAndRemoveFromUnitOfWork(otherTeamPerson);

			var result = target.FindPossibleShiftTrades(myself, true, DateOnly.Today);
				
			result.Should().Contain(myTeamPerson);
			result.Should().Not.Contain(otherTeamPerson);
		}

		private IPerson createPersonInOtherTeam()
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person.WorkflowControlSet = wcs;
			person.AddPersonPeriod(createPersonPeriodOtherTeam());
			return person;
		}

		private IPerson createPersonInMyTeam()
		{
			var person = new Person();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			person.WorkflowControlSet = wcs;
			person.AddPersonPeriod(personPeriodMyTeam);
			return person;
		}

		private IPersonPeriod createPersonPeriodOtherTeam()
		{
			var otherTeam = new Team { Site = mySite, Description = new Description("Other Team") };
			var personPeriodOtherTeam = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, otherTeam);

			PersistAndRemoveFromUnitOfWork(otherTeam);
			PersistAndRemoveFromUnitOfWork(personPeriodOtherTeam.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personPeriodOtherTeam.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriodOtherTeam.PersonContract.PartTimePercentage);

			return personPeriodOtherTeam;
		}
	}
}