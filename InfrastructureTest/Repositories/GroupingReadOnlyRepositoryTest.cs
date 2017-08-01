using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class GroupingReadOnlyRepositoryTest
	{
		public IGroupingReadOnlyRepository Target;
		public WithUnitOfWork WithUnitOfWork;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;

		[Test]
		public void ShouldGroupPagesFromReadModel()
		{ 
			WithUnitOfWork.Do(() =>
			{
				var items = Target.AvailableGroupPages();
				items.Count().Should().Be.EqualTo(0);
			});
		}

		[Test]
		public void ShouldLoadAvailableGroupsWithPageIdFromReadModel()
		{
			WithUnitOfWork.Do(() =>
			{
				var items = Target.AvailableGroups(new ReadOnlyGroupPage { PageId = Group.PageMainId, PageName = "xxMain" },
				DateOnly.Today);
				items.Count().Should().Be.EqualTo(0);
			});
			
		}

		[Test]
		public void ShouldLoadAvailableGroupsFromReadModel()
		{
			WithUnitOfWork.Do(() =>
			{
				var items = Target.AvailableGroups(new ReadOnlyGroupPage { PageName = "xxMain", PageId = Group.PageMainId }, DateOnly.Today);
				items.Count().Should().Be.EqualTo(0);
			});
		}

		[Test]
		public void ShouldLoadDetailsForGroupFromReadModel()
		{
			WithUnitOfWork.Do(() =>
			{
				var items = Target.DetailsForGroup(Guid.Empty, DateOnly.Today);
				items.Count().Should().Be.EqualTo(0);
			});
		}
		[Test]
		public void ShouldLoadDetailsForGroupFromReadModelForRange()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			
			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PersonRepository.Add(personToTest);
			});
			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});
			
			WithUnitOfWork.Do(() =>
			{
				var items = Target.DetailsForGroup(team.Id.GetValueOrDefault(), new DateOnlyPeriod(2001, 1, 1, 2001, 1, 2));
				items.Count().Should().Be.EqualTo(1);
			});
		}

		[Test]
		public void ShouldNotIncludePersonThatLeftTheBusiness()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);
			personToTest.TerminatePerson(new DateOnly(2000, 12, 31), new PersonAccountUpdaterDummy());
			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});
			
			WithUnitOfWork.Do(() =>
			{
				var items = Target.DetailsForGroup(team.Id.GetValueOrDefault(), new DateOnlyPeriod(2001, 1, 1, 2001, 1, 2));
				items.Count().Should().Be.EqualTo(0);
			});
		}

		[Test]
		public void ShouldCallUpdateGroupingReadModelDataWithoutCrash()
		{
			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModelData(new[] { Guid.NewGuid() });
			});
		}

		[Test]
		public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
		{
			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModelGroupPage(new[] { Guid.NewGuid() });
			});
		}

		[Test]
		public void ShouldCallUpdateReadModelWithoutCrash()
		{
			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new[] { Guid.NewGuid() });
			});
		}

		[Test]
		public void ShouldFindDetailFromPersonId()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var items = Target.DetailsForPeople(new[] { personToTest.Id.GetValueOrDefault() });
				items.Count().Should().Be.EqualTo(1);
			});
		}

		[Test]
		public void ShouldIncludePersonThatLeftTheBusinessMidPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);
			personToTest.TerminatePerson(new DateOnly(2001, 1, 3), new PersonAccountUpdaterDummy());
			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var items = Target.DetailsForGroup(team.Id.GetValueOrDefault(), new DateOnlyPeriod(2001, 1, 1, 2001, 1, 5));
				items.Count().Should().Be.EqualTo(1);
			});
		}

		[Test]
		public void ShouldIncludeGroupWithPersonThatLeftTheBusinessMidPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);

			personToTest.TerminatePerson(new DateOnly(2001, 1, 3), new PersonAccountUpdaterDummy());
			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var items = Target.AvailableGroups(new ReadOnlyGroupPage { PageName = "xxMain", PageId = Group.PageMainId }, new DateOnlyPeriod(2001, 1, 1, 2001, 1, 5));
				items.Count().Should().Be.EqualTo(1);
			});
		}

		[Test]
		public void ShouldReturnAvailableGroupPagesBasedOnOneDayPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			var activity = new Activity("dummy activity");
			var skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			var personContract = PersonContractFactory.CreatePersonContract();

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
			});

			var personPeriod = new PersonPeriod(new DateOnly(2017, 6, 1),
												personContract,
												team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(0.44)));
			personToTest.AddPersonPeriod(personPeriod);
			personToTest.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 1, 1),personContract, team));

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 5, 28), new DateOnly(2017, 05, 28));
				var result = Target.AvailableGroupsBasedOnPeriod(period);

				result.Count().Should().Be.EqualTo(4);
			});
		}
		[Test]
		public void ShouldReturnAvailableGroupPagesBasedOnPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");
			var activity = new Activity("dummy activity");
			var skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			var personContract = PersonContractFactory.CreatePersonContract();

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
			});

			var personPeriod = new PersonPeriod(new DateOnly(2017, 6, 1),
												personContract,
												team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(0.44)));
			personToTest.AddPersonPeriod(personPeriod);
			personToTest.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 1, 1),personContract, team));

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 5, 28), new DateOnly(2017, 06, 2));
				var result = Target.AvailableGroupsBasedOnPeriod(period);

				result.Count().Should().Be.EqualTo(5);
			});
		}

		[Test]
		public void ShouldReturnAvailableGroupDetailsBasedOnPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var team = TeamFactory.CreateTeam("Dummy Team", "Dummy Site");
			var activity = new Activity("dummy activity");
			var skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			var personContract = PersonContractFactory.CreatePersonContract();

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
			});

			var personPeriod = new PersonPeriod(new DateOnly(2017, 6, 1),
												personContract,
												team);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(0.44)));
			personToTest.AddPersonPeriod(personPeriod);
			personToTest.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 1, 1), personContract, team));

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 5, 28), new DateOnly(2017, 06, 2));
				var gps = Target.AvailableGroupsBasedOnPeriod(period).ToList();
				var result = Target.AvailableGroups(gps, period).ToLookup(gp => gp.PageId);
				var orgs = result[Group.PageMainId].ToLookup(o => o.SiteId);
				var normalGroups = gps.Where(gp => gp.PageId != Group.PageMainId).OrderBy(gp => gp.PageName);

				orgs.Count().Should().Be.EqualTo(1);
				orgs[team.Site.Id].Count().Should().Be.EqualTo(1);
				orgs[team.Site.Id].Single().GroupName.Should().Be.EqualTo("Dummy Site/Dummy Team");
				result[normalGroups.First().PageId].Single().GroupName.Should().Be.EqualTo("dummyContract");

			});
		}
	}
}
