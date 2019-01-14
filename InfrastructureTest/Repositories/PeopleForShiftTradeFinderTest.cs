using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Teleopti.Ccc.Domain.Collection;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PeopleForShiftTradeFinderTest : DatabaseTest
	{
		private PeopleForShiftTradeFinder target;
		private IContract contract;
		private ISite site;
		private ITeam team;

		[SetUp]
		public void SetUp()
		{
			contract = new Contract("contract1");
			site = SiteFactory.CreateSimpleSite("d");
			team = createAndPersistTeam(site);
			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(site);
		}

		[Test]
		public void ShouldGetPersonForDayAndTeam()
		{
			var workflowControlSet = createAndPersistWorkflowControlSet();

			var per1 = createPersonWithSkill(team, workflowControlSet);
			var per2 = createPersonWithSkill(team, workflowControlSet);
			var per3 = createPersonWithSkill(team, workflowControlSet);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 2, per2, per3);
		}

		[Test]
		public void ShouldFilterPersonWithoutMustHaveSkill()
		{
			var skills = createAndPersistSkills();

			var workflowControlSet = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithSkill(team, workflowControlSet, skills[0]);
			var per2 = createPersonWithSkill(team, workflowControlSet);
			var per3 = createPersonWithSkill(team, workflowControlSet, skills[0]);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 1, per3);
		}

		[Test]
		public void ShouldFilterPersonWithMustHaveSkil()
		{
			var skills = createAndPersistSkills();

			var workflowControlSet = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithSkill(team, workflowControlSet);
			var per2 = createPersonWithSkill(team, workflowControlSet);
			var per3 = createPersonWithSkill(team, workflowControlSet, skills[0]);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 1, per2);
		}

		[Test]
		public void ShouldFilterPersonWithInactiveMustHaveSkil()
		{
			var skills = createAndPersistSkills();

			var workflowControlSet = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithSkill(team, workflowControlSet, skills[0]);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per3 = createPersonWithSkill(team, workflowControlSet, skills[0]);

			var personPeriod2 = createAndPersistPersonPeriod(per2, team, skills[0]);

			((PersonSkill)personPeriod2.PersonSkillCollection.Single()).Active = false;

			persistReadModel(per2, team, site);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 1, per3);
		}

		[Test]
		public void ShouldFilterPersonWithoutSkillFromMergedWCSMustHaveSkills()
		{
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[0]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[1]);

			var per1 = createPersonWithSkill(team, workflowControlSetA, skills);
			var per2 = createPersonWithSkill(team, workflowControlSetB, skills);
			var per3 = createPersonWithSkill(team, workflowControlSetB, skills[1]);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 1, per2);
		}

		[Test]
		public void ShouldFilterOutWhenFromDoesntHaveSkillFromMergedWCSMustHaveSkills()
		{
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[1]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithSkill(team, workflowControlSetA, skills[0]);
			var per2 = createPersonWithSkill(team, workflowControlSetB, skills);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 0);
		}

		[Test]
		public void ShouldFilterPersonWhenDifferentSkillsInMergedMatchingSkills()
		{
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[0], skills[1]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[1], skills[2]);

			var per1 = createPersonWithSkill(team, workflowControlSetA, skills);
			var per2 = createPersonWithSkill(team, workflowControlSetB, skills[0], skills[1]);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 0);
		}

		[Test]
		public void ShouldFilterPersonWithoutAnySkillSameAsPersonFromAndDifferentSkillsInMergedMatchingSkills()
		{
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[0], skills[1]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[2]);

			var per1 = createPersonWithSkill(team, workflowControlSetA, skills[1], skills[2]);
			var per2 = createPersonWithSkill(team, workflowControlSetB, skills[0]);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 0);
		}

		[Test]
		public void ShouldNotFilterPersonWhenDifferentSkillsNotInMergedMatchingSkills()
		{
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[0], skills[1]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[1]);

			var per1 = createPersonWithSkill(team, workflowControlSetA, skills[0], skills[2]);
			var per2 = createPersonWithSkill(team, workflowControlSetB, skills[0]);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 1, per2);
		}

		[Test]
		public void ShouldNotFilterPersonWhenDifferentSkillsIsEmpty()
		{
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[0], skills[1]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[2]);

			var per1 = createPersonWithSkill(team, workflowControlSetA, skills[2]);
			var per2 = createPersonWithSkill(team, workflowControlSetB, skills[2]);

			var result = invokeGetPeople(per1, team);
			assertPeopleResult(result, 1, per2);
		}

		[Test]
		public void ShouldFilterPersonWithoutSkill()
		{
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet();
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithSkill(team, workflowControlSetA, skills[0]);
			var per2 = createPersonWithSkill(team, workflowControlSetB);

			var result = invokeGetPeople(per2, team);
			assertPeopleResult(result, 0);
		}

		private IPerson createPersonWithSkill(ITeam team, IWorkflowControlSet workflowControlSet, params ISkill[] skills)
		{
			var person = createPersonWithWorkflowControlSet(workflowControlSet);
			createAndPersistPersonPeriod(person, team, skills);
			persistReadModel(person, team, site);
			return person;
		}

		private IList<IPersonAuthorization> invokeGetPeople(IPerson personFrom, ITeam team)
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());
			var result = target.GetPeople(personFrom, new DateOnly(2017, 3, 23), new List<Guid> {team.Id.Value}, "");
			return result;
		}

		private static void assertPeopleResult(IList<IPersonAuthorization> result, int expectedCount,
			params IPerson[] expectedPeople)
		{
			result.ToArray().Length.Should().Be.EqualTo(expectedCount);
			foreach (var expectedPerson in expectedPeople)
			{
				result.Any(r => r.PersonId.Equals(expectedPerson.Id.Value)).Should().Be(true);
			}
		}

		private Team createAndPersistTeam(ISite site)
		{
			PersistAndRemoveFromUnitOfWork(site);
			var team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			PersistAndRemoveFromUnitOfWork(team);
			return team;
		}

		private ISkill[] createAndPersistSkills()
		{
			var type = SkillTypeFactory.CreateSkillTypePhone();
			PersistAndRemoveFromUnitOfWork(type);

			var activity = new Activity("test");
			PersistAndRemoveFromUnitOfWork(activity);

			var swedish = SkillFactory.CreateSkill("Swedish", type, 15);
			swedish.TimeZone = TimeZoneInfo.GetSystemTimeZones()[1];
			swedish.Activity = activity;
			PersistAndRemoveFromUnitOfWork(swedish);

			var english = SkillFactory.CreateSkill("English", type, 15);
			english.TimeZone = TimeZoneInfo.GetSystemTimeZones()[1];
			english.Activity = activity;
			PersistAndRemoveFromUnitOfWork(english);

			var spanish = SkillFactory.CreateSkill("Spanish", type, 15);
			spanish.TimeZone = TimeZoneInfo.GetSystemTimeZones()[1];
			spanish.Activity = activity;
			PersistAndRemoveFromUnitOfWork(spanish);

			return new[] {swedish, english, spanish};
		}

		private static IPerson createPersonWithWorkflowControlSet(IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePerson("a", "b");
			person.WorkflowControlSet = workflowControlSet;
			return person;
		}

		private IPersonPeriod createAndPersistPersonPeriod(IPerson person, ITeam team, params ISkill[] skills)
		{
			var personPeriod = new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract), team);
			skills.ForEach(skill =>
			{
				var personSkill = new PersonSkill(skill, new Percent(100));
				personPeriod.AddPersonSkill(personSkill);
			});
			person.AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private IWorkflowControlSet createAndPersistWorkflowControlSet(params ISkill[] skills)
		{
			var workflowControlSet = new WorkflowControlSet("d")
			{
				SchedulePublishedToDate = new DateTime(2000, 1, 10),
				PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11),
				PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11)
			};

			skills.ForEach(skill =>
			{
				workflowControlSet.AddSkillToMatchList(skill);
			});

			PersistAndRemoveFromUnitOfWork(workflowControlSet);

			return workflowControlSet;
		}

		private IPersonContract createPersonContract(IContract contract, IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract(contract);
			if (otherBusinessUnit != null)
			{
				pContract.Contract.SetBusinessUnit(otherBusinessUnit);
				pContract.ContractSchedule.SetBusinessUnit(otherBusinessUnit);
				pContract.PartTimePercentage.SetBusinessUnit(otherBusinessUnit);
			}
			PersistAndRemoveFromUnitOfWork(pContract.Contract);
			PersistAndRemoveFromUnitOfWork(pContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(pContract.PartTimePercentage);
			return pContract;
		}

		private void persistReadModel(IPerson person, ITeam team, ISite site)
		{
			PersistAndRemoveFromUnitOfWork(person);

			const string populateReadModelQuery
				= "INSERT INTO [ReadModel].[GroupingReadOnly]" +
				  "([PersonId],[StartDate],[TeamId],[SiteId],[BusinessUnitId] ,[GroupId],[PageId])" +
				  "VALUES (:personId,:startDate,:teamId,:siteId,:businessUnitId,:groupId,:pageId)";

			Session.CreateSQLQuery(populateReadModelQuery)
				.SetGuid("personId", person.Id.Value)
				.SetDateTime("startDate", new DateTime(2012, 02, 02))
				.SetGuid("teamId", team.Id.Value)
				.SetGuid("siteId", site.Id.Value)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetGuid("groupId", team.Id.Value)
				.SetGuid("pageId", Guid.NewGuid())
				.ExecuteUpdate();
		}

		private Guid getBusinessUnitId()
		{
			return ((ITeleoptiIdentity) ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
		}
	}
}
