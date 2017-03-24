using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Castle.Core.Internal;
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
using Teleopti.Interfaces.Domain;
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

		[SetUp]
		public void SetUp()
		{
			contract = new Contract("contract1");
			site = SiteFactory.CreateSimpleSite("d");
			PersistAndRemoveFromUnitOfWork(contract);
			PersistAndRemoveFromUnitOfWork(site);
		}

		[Test]
		public void ShouldGetPersonForDayAndTeam()
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());

			var team = createAndPersistTeam(site);
			var workflowControlSet = createAndPersistWorkflowControlSet();

			var per1 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per3 = createPersonWithWorkflowControlSet(workflowControlSet);

			createAndPersistPersonPeriod(per1, team);
			createAndPersistPersonPeriod(per2, team);
			createAndPersistPersonPeriod(per3, team);

			persistReadModel(per1, team, site);
			persistReadModel(per2, team, site);
			persistReadModel(per3, team, site);

			var result = target.GetPeople(per1, new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "").ToList();
			result.Count.Should().Be.EqualTo(2);
			result.Any(person => person.PersonId == per2.Id).Should().Be.True();
			result.Any(person => person.PersonId == per3.Id).Should().Be.True();
		}

		[Test]
		public void ShouldFilterPersonWithoutMustHaveSkill()
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());

			var team = createAndPersistTeam(site);
			var skills = createAndPersistSkills();

			var workflowControlSet = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per3 = createPersonWithWorkflowControlSet(workflowControlSet);

			createAndPersistPersonPeriod(per1, team, skills[0]);
			createAndPersistPersonPeriod(per2, team);
			createAndPersistPersonPeriod(per3, team, skills[0]);

			persistReadModel(per1, team, site);
			persistReadModel(per2, team, site);
			persistReadModel(per3, team, site);

			var result = target.GetPeople(per1, new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "");

			result.Count.Should().Be.EqualTo(1);
			result.Any(r => r.PersonId.Equals(per3.Id.Value)).Should().Be(true);
		}

		[Test]
		public void ShouldFilterPersonWithMustHaveSkil()
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());

			var team = createAndPersistTeam(site);

			var skills = createAndPersistSkills();

			var workflowControlSet = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per3 = createPersonWithWorkflowControlSet(workflowControlSet);

			createAndPersistPersonPeriod(per1, team, skills[0]);
			createAndPersistPersonPeriod(per2, team);
			createAndPersistPersonPeriod(per3, team, skills[0]);

			persistReadModel(per1, team, site);
			persistReadModel(per2, team, site);
			persistReadModel(per3, team, site);

			var result = target.GetPeople(per1, new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "");
			result.ToArray().Length.Should().Be.EqualTo(1);
			result.Any(r => r.PersonId.Equals(per3.Id.Value)).Should().Be(true);

		}


		[Test]
		public void ShouldFilterPersonWithInactiveMustHaveSkil()
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());

			var team = createAndPersistTeam(site);

			var skills = createAndPersistSkills();

			var workflowControlSet = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSet);
			var per3 = createPersonWithWorkflowControlSet(workflowControlSet);

			createAndPersistPersonPeriod(per1, team, skills[0]);
			var personPeriod2 = createAndPersistPersonPeriod(per2, team, skills[0]);
			createAndPersistPersonPeriod(per3, team, skills[0]);

			((PersonSkill)personPeriod2.PersonSkillCollection.Single()).Active = false;

			persistReadModel(per1, team, site);
			persistReadModel(per2, team, site);
			persistReadModel(per3, team, site);

			var result = target.GetPeople(per1, new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "");
			result.ToArray().Length.Should().Be.EqualTo(1);

			result.Any(r => r.PersonId.Equals(per3.Id.Value)).Should().Be(true);
		}

		[Test]
		public void ShouldFilterPersonWithoutSkillFromMergedWCSMustHaveSkills()
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());

			var team = createAndPersistTeam(site);
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[0]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[1]);

			var per1 = createPersonWithWorkflowControlSet(workflowControlSetA);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSetB);
			var per3 = createPersonWithWorkflowControlSet(workflowControlSetB);

			createAndPersistPersonPeriod(per1, team, skills);
			createAndPersistPersonPeriod(per2, team, skills);
			createAndPersistPersonPeriod(per3, team, skills[1]);

			persistReadModel(per1, team, site);
			persistReadModel(per2, team, site);
			persistReadModel(per3, team, site);

			var result = target.GetPeople(per1, new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "");
			result.ToArray().Length.Should().Be.EqualTo(1);

			result.Any(r => r.PersonId.Equals(per2.Id.Value)).Should().Be(true);
		}



		[Test]
		public void ShouldFilterOutWhenFromDoesntHaveSkillFromMergedWCSMustHaveSkills()
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());

			var team = createAndPersistTeam(site);
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[1]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[0]);

			var per1 = createPersonWithWorkflowControlSet(workflowControlSetA);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSetB);


			createAndPersistPersonPeriod(per1, team, skills[0]); //per1 only has one skill
			createAndPersistPersonPeriod(per2, team, skills);


			persistReadModel(per1, team, site);
			persistReadModel(per2, team, site);


			var result = target.GetPeople(per1, new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "");
			result.ToArray().Length.Should().Be.EqualTo(0);

		}

		[Test]
		public void ShouldFilterOutWhenFromAndToDoesNotHaveMatchingSkills()
		{
			target = new PeopleForShiftTradeFinder(CurrentUnitOfWork.Make());

			var team = createAndPersistTeam(site);
			var skills = createAndPersistSkills();

			var workflowControlSetA = createAndPersistWorkflowControlSet(skills[0], skills[1]);
			var workflowControlSetB = createAndPersistWorkflowControlSet(skills[1], skills[2]);

			var per1 = createPersonWithWorkflowControlSet(workflowControlSetA);
			var per2 = createPersonWithWorkflowControlSet(workflowControlSetB);


			createAndPersistPersonPeriod(per1, team, skills); 
			createAndPersistPersonPeriod(per2, team, skills[0], skills[1]);

			persistReadModel(per1, team, site);
			persistReadModel(per2, team, site);

			var result = target.GetPeople(per1, new DateOnly(2017, 3, 23), new List<Guid> { team.Id.Value }, "");
			result.ToArray().Length.Should().Be.EqualTo(0);
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
			var type = SkillTypeFactory.CreateSkillType();
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


			return new[] { swedish, english, spanish };
		}

		private static IPerson createPersonWithWorkflowControlSet(IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePerson("a", "b");
			person.WorkflowControlSet = workflowControlSet;
			return person;
		}

		private IPersonPeriod createAndPersistPersonPeriod(IPerson person, Team team, params ISkill[] skills)
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

			const string populateReadModelQuery = @"INSERT INTO [ReadModel].[GroupingReadOnly] 
                ([PersonId],[StartDate],[TeamId],[SiteId],[BusinessUnitId] ,[GroupId],[PageId])
			 VALUES (:personId,:startDate,:teamId,:siteId,:businessUnitId,:groupId,:pageId)";

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
			return ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
		}
	}
}
