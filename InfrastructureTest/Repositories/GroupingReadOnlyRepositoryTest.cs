using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class GroupingReadOnlyRepositoryTest
	{
		public IGroupingReadOnlyRepository Target;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public ICurrentBusinessUnit CurrentBusinessUnit;

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
		public void ShouldFindGroupsWithGroupIdBasedOnPeriodFromReadModel()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent0");
			var personToTest1 = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var personContract = PersonContractFactory.CreatePersonContract();
			var personContract1 = PersonContractFactory.CreatePersonContract("anotherContract", "contractSchedule", "partTimePercentage");
			var personPeriod = new PersonPeriod(new DateOnly(2000, 1, 1),
				personContract,
				team);
			personToTest.AddPersonPeriod(personPeriod);
			personToTest1.AddPersonPeriod(new PersonPeriod(new DateOnly(2000, 1, 1),
				personContract1,
				team));

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractRepository.Add(personContract1.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				ContractScheduleRepository.Add(personContract1.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PartTimePercentageRepository.Add(personContract1.PartTimePercentage);
				PersonRepository.Add(personToTest);
				PersonRepository.Add(personToTest1);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { personToTest.Id.Value, personToTest1.Id.Value });
			});

			WithUnitOfWork.Do(() =>
			{
				var items = Target.FindGroups(new List<Guid>
				{
					personContract.Contract.Id.GetValueOrDefault(),
					personContract1.Contract.Id.GetValueOrDefault()
				}, new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1));

				items.Count().Should().Be.EqualTo(2);
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
				var result = Target.AvailableGroups(period, gps.Select(gp => gp.PageId).ToArray())
				.ToLookup(gp => gp.PageId);
				var orgs = result[Group.PageMainId].ToLookup(o => o.SiteId);
				var normalGroups = gps.Where(gp => gp.PageId != Group.PageMainId).OrderBy(gp => gp.PageName);

				orgs.Count().Should().Be.EqualTo(1);
				orgs[team.Site.Id].Count().Should().Be.EqualTo(1);
				orgs[team.Site.Id].Single().GroupName.Should().Be.EqualTo("Dummy Site/Dummy Team");
				orgs[team.Site.Id].Single().PersonId.Should().Be.EqualTo(personToTest.Id);
				result[normalGroups.First().PageId].Single().GroupName.Should().Be.EqualTo("dummyContract");

			});
		}

		[Test]
		public void ShouldReturnAvailableGroupDetailsWhoIsTerminatedLaterThanPeriodStart()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var anotherPerson = PersonFactory.CreatePerson("dummyAgent2");
			var team = TeamFactory.CreateTeam("Dummy Team 1", "Dummy Site");
			var teamAnother = TeamFactory.CreateSimpleTeam("Dummy Team 2");
			teamAnother.Site = team.Site;
			var activity = new Activity("dummy activity");
			var skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			var personContract = PersonContractFactory.CreatePersonContract();

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				TeamRepository.Add(teamAnother);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
			});

			personToTest.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 1, 1), personContract, team));
			personToTest.TerminatePerson(new DateOnly(2017, 7, 1), new PersonAccountUpdaterDummy());
			anotherPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 1, 1), personContract, teamAnother));
			anotherPerson.TerminatePerson(new DateOnly(2017, 5, 31), new PersonAccountUpdaterDummy());

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(personToTest);
				PersonRepository.Add(anotherPerson);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 5, 28), new DateOnly(2017, 06, 2));
				var gps = Target.AvailableGroupsBasedOnPeriod(period).ToList();
				var result = Target.AvailableGroups(period, gps.Select(gp => gp.PageId).ToArray())
				.ToLookup(gp => gp.PageId);
				var orgs = result[Group.PageMainId].ToLookup(o => o.SiteId);
				var normalGroups = gps.Where(gp => gp.PageId != Group.PageMainId).OrderBy(gp => gp.PageName);

				orgs.Count().Should().Be.EqualTo(1);
				orgs[team.Site.Id].Count().Should().Be.EqualTo(2);
				orgs[team.Site.Id].First().GroupName.Should().Be.EqualTo("Dummy Site/Dummy Team 1");
				orgs[team.Site.Id].Second().GroupName.Should().Be.EqualTo("Dummy Site/Dummy Team 2");
				result[normalGroups.First().PageId].Count().Should().Be.EqualTo(2);
				result[normalGroups.First().PageId].First().GroupName.Should().Be.EqualTo("dummyContract");

				period = new DateOnlyPeriod(new DateOnly(2017, 6, 1), new DateOnly(2017, 6, 2));
				result = Target.AvailableGroups(period, gps.Select(gp => gp.PageId).ToArray()).ToLookup(gp => gp.PageId);
				orgs = result[Group.PageMainId].ToLookup(o => o.SiteId);
				normalGroups = gps.Where(gp => gp.PageId != Group.PageMainId).OrderBy(gp => gp.PageName);

				orgs.Count().Should().Be.EqualTo(1);
				orgs[team.Site.Id].Count().Should().Be.EqualTo(1);
				orgs[team.Site.Id].First().GroupName.Should().Be.EqualTo("Dummy Site/Dummy Team 1");
				result[normalGroups.First().PageId].Count().Should().Be.EqualTo(1);
				result[normalGroups.First().PageId].First().GroupName.Should().Be.EqualTo("dummyContract");
			});
		}

		[Test]
		public void ShouldReturnAvailaleGroupDetailsForMutilpleGroupsBasedOnPeriod()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var anotherPerson = PersonFactory.CreatePerson("dummyAgent2");
			var team = TeamFactory.CreateTeam("Dummy Team", "Dummy Site");
			var teamAnother = TeamFactory.CreateSimpleTeam("Dummy Team 2");
			teamAnother.Site = team.Site;
			var activity = new Activity("dummy activity");
			var skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			var personContract = PersonContractFactory.CreatePersonContract();

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				TeamRepository.Add(teamAnother);
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

			anotherPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 7, 1), personContract, teamAnother));

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(personToTest);
				PersonRepository.Add(anotherPerson);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 5, 28), new DateOnly(2017, 06, 2));

				var result = Target.DetailsForGroups(new[] { personContract.Contract.Id.Value }, period);
				var personIds = result.Select(d => d.PersonId).Distinct();
				result.Count().Should().Be.EqualTo(2);
				personIds.Count().Should().Be.EqualTo(1);
				result.First().FirstName.Should().Be.EqualTo("dummyAgent1");
				result.First().PersonId.Should().Be.EqualTo(personToTest.Id.Value);
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2016, 11, 28), new DateOnly(2016, 12, 28));

				var result = Target.DetailsForGroups(new[] { personContract.Contract.Id.Value }, period);
				result.Count().Should().Be.EqualTo(0);
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 7, 1), new DateOnly(2017, 12, 28));

				var result = Target.DetailsForGroups(new[] { personContract.Contract.Id.Value }, period);
				result.Count().Should().Be.EqualTo(2);
				result.Select(p => p.PersonId).Distinct().Should().Be.Equals(2);

				result.Select(p => p.PersonId).Contains(personToTest.Id.Value).Should().Be.EqualTo(true);
			});
		}

		[Test]
		public void ShouldReturnAvailableGroupDetailsBasedOnSearchCriteria()
		{
			var person1 = PersonFactory.CreatePerson("dummyAgent1");
			var person2 = PersonFactory.CreatePerson("dummyAgent2");
			var team1 = TeamFactory.CreateTeam("Dummy Team", "Dummy Site");
			var team2 = TeamFactory.CreateSimpleTeam("Dummy Team 2");
			team2.Site = team1.Site;
			var activity = new Activity("dummy activity");
			var skill = SkillFactory.CreateSkill("dummy skill");
			skill.Activity = activity;
			var personContract = PersonContractFactory.CreatePersonContract();

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team1.Site);
				TeamRepository.Add(team1);
				TeamRepository.Add(team2);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
			});

			var personPeriod = new PersonPeriod(new DateOnly(2017, 6, 1),
												personContract,
												team1);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(0.44)));
			person1.AddPersonPeriod(personPeriod);
			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 1, 1), personContract, team1));

			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2017, 7, 1), personContract, team2));

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person1);
				PersonRepository.Add(person2);
			});

			WithUnitOfWork.Do(() =>
			{
				Target.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 5, 28), new DateOnly(2017, 06, 2));

				var result = Target.DetailsForGroups(new[] { personContract.Contract.Id.Value }, period);
				var personIds = result.Select(d => d.PersonId).Distinct();
				result.Count().Should().Be.EqualTo(2);
				personIds.Count().Should().Be.EqualTo(1);
				result.First().FirstName.Should().Be.EqualTo("dummyAgent1");
				result.First().PersonId.Should().Be.EqualTo(person1.Id.Value);
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2016, 11, 28), new DateOnly(2016, 12, 28));

				var result = Target.DetailsForGroups(new[] { personContract.Contract.Id.Value }, period);
				result.Count().Should().Be.EqualTo(0);
			});

			WithUnitOfWork.Do(() =>
			{
				var period = new DateOnlyPeriod(new DateOnly(2017, 7, 1), new DateOnly(2017, 12, 28));

				var result = Target.DetailsForGroups(new[] { personContract.Contract.Id.Value }, period);
				result.Count().Should().Be.EqualTo(2);
				result.Select(p => p.PersonId).Distinct().Should().Be.Equals(2);

				result.Select(p => p.PersonId).Contains(person1.Id.Value).Should().Be.EqualTo(true);
			});
		}

		[Test]
		public void ShouldReturnAvailableGroups()
		{
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");
			var team = TeamFactory.CreateTeam("Dummy Team 1", "Dummy Site");
			var activity = new Activity("dummy activity");
			var skill = SkillFactory.CreateSkill("dummy skill");
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 100);
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

			var personPeriod = new PersonPeriod(new DateOnly(2018, 1, 1), personContract, team);
			personPeriod.AddPersonSkill(personSkill);
			personToTest.AddPersonPeriod(personPeriod);
			personToTest.TerminatePerson(new DateOnly(2018, 12, 1), new PersonAccountUpdaterDummy());

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
				var period = new DateOnlyPeriod(new DateOnly(2017, 8, 8), new DateOnly(2018 , 8, 12));
				var result = Target.AllAvailableGroups(period);

				result.Count().Should().Be.EqualTo(5);
				result.First().BusinessUnitId.Should().Be.EqualTo(CurrentBusinessUnit.CurrentId());

				var teamGroup = result.Single(g => g.TeamId == team.Id && g.PageId == Group.PageMainId);
				teamGroup.SiteId.Should().Be.EqualTo(team.Site.Id);
				teamGroup.GroupName.Should().Be.EqualTo("Dummy Site/Dummy Team 1");
				teamGroup.PageId.Should().Be.EqualTo(Group.PageMainId);
				teamGroup.PersonId.Should().Be.EqualTo(personToTest.Id);

				var skillGroup = result.Single(s => s.GroupId == skill.Id);
				skillGroup.GroupName.Should().Be.EqualTo("dummy skill");
				skillGroup.PageName.Should().Be.EqualTo("xxSkill");

				var contractGroup = result.Single(g => g.GroupId == personContract.Contract.Id);
				contractGroup.GroupName.Should().Be.EqualTo("dummyContract");
				contractGroup.PageName.Should().Be.EqualTo("xxContract");

				var contractScheduleGroup = result.Single(g => g.GroupId == personContract.ContractSchedule.Id);
				contractScheduleGroup.GroupName.Should().Be.EqualTo("dummyBasicSchedule");
				contractScheduleGroup.PageName.Should().Be.EqualTo("xxContractSchedule");

				var contractParttimeGroup = result.Single(g => g.GroupId == personContract.PartTimePercentage.Id);
				contractParttimeGroup.GroupName.Should().Be.EqualTo("dummyPartTime 75%");
				contractParttimeGroup.PageName.Should().Be.EqualTo("xxPartTimePercentage");

			});

		
		}


	}
}
