using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class GroupingReadOnlyBug46991Test : DatabaseTest
	{
		private GroupingReadOnlyRepository _target;

		[SetUp]
		public void SetUp()
		{
			_target = new GroupingReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		[Test]
		public void ShouldHandleRenamedTeam()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateGroupingReadModel(new[] { personToTest.Id.Value });

			team.SetDescription(new Description("New team name"));
			PersistAndRemoveFromUnitOfWork(team);
			_target.UpdateGroupingReadModelData(new[] { team.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);
			
			var result = _target.AvailableGroups(date.ToDateOnlyPeriod(),Group.PageMainId).Single();
			result.GroupName.Should().Contain(team.Description.Name);
		}

		[Test]
		public void ShouldHandleRenamedSite()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateGroupingReadModel(new[] { personToTest.Id.Value });

			team.Site.SetDescription(new Description("New site name"));
			PersistAndRemoveFromUnitOfWork(team.Site);
			_target.UpdateGroupingReadModelData(new[] { team.Site.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);

			var result = _target.AvailableGroups(date.ToDateOnlyPeriod(), Group.PageMainId).Single();
			result.GroupName.Should().Contain(team.Site.Description.Name);
		}

		[Test]
		public void ShouldHandleTeamMovedSite()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			PersistAndRemoveFromUnitOfWork(personToTest);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			var newSite = SiteFactory.CreateSimpleSite("New site");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(newSite);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2017, 1, 1),
				personContract,
				team);

			personToTest.AddPersonPeriod(personPeriod);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(personPeriod.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateGroupingReadModel(new[] { personToTest.Id.Value });

			team.Site = newSite;
			PersistAndRemoveFromUnitOfWork(team);
			_target.UpdateGroupingReadModelData(new[] { team.Id.GetValueOrDefault() });

			var date = new DateOnly(2017, 4, 1);

			var result = _target.AvailableGroups(date.ToDateOnlyPeriod(), Group.PageMainId).Single();
			result.GroupName.Should().Contain(team.Site.Description.Name);
		}
	}
}