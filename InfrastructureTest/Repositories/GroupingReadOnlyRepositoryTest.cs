using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class GroupingReadOnlyRepositoryTest : DatabaseTest
	{
		private IGroupingReadOnlyRepository _target;

		protected override void SetupForRepositoryTest()
		{
			_target = new GroupingReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		[Test]
		public void ShouldGroupPagesFromReadModel()
		{
			var items = _target.AvailableGroupPages();
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAvailableGroupsWithPageIdFromReadModel()
		{
			var items = _target.AvailableGroups(new ReadOnlyGroupPage {PageId = Group.PageMainId, PageName = "xxMain"},
				DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAvailableGroupsFromReadModel()
		{
			var items = _target.AvailableGroups(new ReadOnlyGroupPage { PageName = "xxMain",PageId = Group.PageMainId}, DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadDetailsForGroupFromReadModel()
		{
			var items = _target.DetailsForGroup(Guid.Empty, DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadDetailsForGroupFromReadModelForRange()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);

			PersistAndRemoveFromUnitOfWork(personContract.Contract);
			PersistAndRemoveFromUnitOfWork(personContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personContract.PartTimePercentage);
			
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			
			var items = _target.DetailsForGroup(team.Id.GetValueOrDefault(), new DateOnlyPeriod(2001,1,1,2001,1,2));
			items.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotIncludePersonThatLeftTheBusiness()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			
			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);
			
			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);
			
			PersistAndRemoveFromUnitOfWork(personContract.Contract);
			PersistAndRemoveFromUnitOfWork(personContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personContract.PartTimePercentage);
			
			personToTest.TerminatePerson(new DateOnly(2000, 12, 31), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateGroupingReadModel(new List<Guid> {Guid.Empty});

			var items = _target.DetailsForGroup(team.Id.GetValueOrDefault(), new DateOnlyPeriod(2001, 1, 1, 2001, 1, 2));
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldIncludePersonThatLeftTheBusinessMidPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);

			PersistAndRemoveFromUnitOfWork(personContract.Contract);
			PersistAndRemoveFromUnitOfWork(personContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personContract.PartTimePercentage);

			personToTest.TerminatePerson(new DateOnly(2001, 1, 3), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });

			var items = _target.DetailsForGroup(team.Id.GetValueOrDefault(), new DateOnlyPeriod(2001, 1, 1, 2001, 1, 5));
			items.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIncludeGroupWithPersonThatLeftTheBusinessMidPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			PersistAndRemoveFromUnitOfWork(team.Site);
			PersistAndRemoveFromUnitOfWork(team);

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);

			PersistAndRemoveFromUnitOfWork(personContract.Contract);
			PersistAndRemoveFromUnitOfWork(personContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(personContract.PartTimePercentage);

			personToTest.TerminatePerson(new DateOnly(2001, 1, 3), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(personToTest);

			_target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });

			var items = _target.AvailableGroups(new ReadOnlyGroupPage { PageName = "xxMain", PageId = Group.PageMainId }, new DateOnlyPeriod(2001, 1, 1, 2001, 1, 5));
			items.Count().Should().Be.EqualTo(1);
		}

		[Test]
        public void ShouldCallUpdateReadModelWithoutCrash()
        {
            _target.UpdateGroupingReadModel(new[] { Guid.NewGuid() });
        }

        [Test]
        public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
        {
            _target.UpdateGroupingReadModelGroupPage(new[] { Guid.NewGuid() });
        }

        [Test]
        public void ShouldCallUpdateGroupingReadModelDataWithoutCrash()
        {
            _target.UpdateGroupingReadModelData(new[] { Guid.NewGuid() });
        }
	}
}
