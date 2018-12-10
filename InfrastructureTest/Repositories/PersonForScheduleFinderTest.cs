using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	class PersonForScheduleFinderTest : DatabaseTest
	{
		private PersonForScheduleFinder target;

		private ISite site;
		private ITeam team;
		private IContract contract1;
		private IContract contract2;
		private IPerson per1;
		private IPerson per2;
		private IPerson per3;	

		[Test]
		public void ShouldGetPersonForDayAndTeam()
		{
			target = new PersonForScheduleFinder(CurrentUnitOfWork.Make());

			site = SiteFactory.CreateSimpleSite("d");

			PersistAndRemoveFromUnitOfWork(site);
			team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			PersistAndRemoveFromUnitOfWork(team);

			contract1 = new Contract("contract1");
			contract2 = new Contract("contract2");

			PersistAndRemoveFromUnitOfWork(contract1);
			PersistAndRemoveFromUnitOfWork(contract2);

			per1 = PersonFactory.CreatePerson("roger", "kratz");
			per2 = PersonFactory.CreatePerson("z", "balog");
			per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract1), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));


			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 10);
			workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);

			PersistAndRemoveFromUnitOfWork(workflowControlSet);

			per1.WorkflowControlSet = workflowControlSet;
			per2.WorkflowControlSet = workflowControlSet;
			per3.WorkflowControlSet = workflowControlSet;

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			persistReadModel(per1.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());
			persistReadModel(per2.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());
			persistReadModel(per3.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());

			var result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "");
			result.ToArray().Length.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetPeopleForDayAndTeamWithLargeInputList()
		{
			target = new PersonForScheduleFinder(CurrentUnitOfWork.Make());

			site = SiteFactory.CreateSimpleSite("d");

			PersistAndRemoveFromUnitOfWork(site);
			team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			PersistAndRemoveFromUnitOfWork(team);

			contract1 = new Contract("contract1");
			contract2 = new Contract("contract2");

			PersistAndRemoveFromUnitOfWork(contract1);
			PersistAndRemoveFromUnitOfWork(contract2);

			per1 = PersonFactory.CreatePerson("roger", "kratz");
			per2 = PersonFactory.CreatePerson("z", "balog");
			per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract1), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));


			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 10);
			workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);

			PersistAndRemoveFromUnitOfWork(workflowControlSet);

			per1.WorkflowControlSet = workflowControlSet;
			per2.WorkflowControlSet = workflowControlSet;
			per3.WorkflowControlSet = workflowControlSet;

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			persistReadModel(per1.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());
			persistReadModel(per2.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());
			persistReadModel(per3.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());

			var bigGroupList = new List<Guid>();
			for (var i = 0; i < 4000; i++)
			{
				bigGroupList.Add(Guid.NewGuid());
			}
			bigGroupList.Add(team.Id.Value);

			var result = target.GetPersonFor(new DateOnly(2012, 2, 2), bigGroupList, "");
			result.ToArray().Length.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldFilterPersonByNameSegment()
		{
			target = new PersonForScheduleFinder(CurrentUnitOfWork.Make());

			site = SiteFactory.CreateSimpleSite("d");

			PersistAndRemoveFromUnitOfWork(site);
			team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			PersistAndRemoveFromUnitOfWork(team);

			contract1 = new Contract("contract1");
			contract2 = new Contract("contract2");

			PersistAndRemoveFromUnitOfWork(contract1);
			PersistAndRemoveFromUnitOfWork(contract2);

			per1 = PersonFactory.CreatePerson("roger", "kratz");
			per2 = PersonFactory.CreatePerson("z", "balog");
			per3 = PersonFactory.CreatePerson("小红", "小白");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract1), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));


			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 10);
			workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);

			PersistAndRemoveFromUnitOfWork(workflowControlSet);

			per1.WorkflowControlSet = workflowControlSet;
			per2.WorkflowControlSet = workflowControlSet;
			per3.WorkflowControlSet = workflowControlSet;

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);
			persistReadModel(per1.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());
			persistReadModel(per2.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());
			persistReadModel(per3.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), team.Id.GetValueOrDefault());

			var result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> {team.Id.Value}, "roger");
			result.ToArray().Length.Should().Be.EqualTo(1);            

			result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "roger k");
			result.ToArray().Length.Should().Be.EqualTo(1);

			result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "kratz rog");
			result.ToArray().Length.Should().Be.EqualTo(0);

            result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "红小白");
            result.ToArray().Length.Should().Be.EqualTo(1);

            result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "小红 白");
            result.ToArray().Length.Should().Be.EqualTo(0);

            result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { team.Id.Value }, "rogerk");
            result.ToArray().Length.Should().Be.EqualTo(0);
        }

		[Test]
		public void ShouldGetPersonForDayAndGroup()
		{
			target = new PersonForScheduleFinder(CurrentUnitOfWork.Make());

			site = SiteFactory.CreateSimpleSite("d");

			PersistAndRemoveFromUnitOfWork(site);
			team = TeamFactory.CreateSimpleTeam();
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			PersistAndRemoveFromUnitOfWork(team);

			contract1 = new Contract("contract1");
			contract2 = new Contract("contract2");

			PersistAndRemoveFromUnitOfWork(contract1);
			PersistAndRemoveFromUnitOfWork(contract2);

			per1 = PersonFactory.CreatePerson("roger", "kratz");
			per2 = PersonFactory.CreatePerson("z", "balog");
			per3 = PersonFactory.CreatePerson("a", "balog");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract1), team));
			per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));
			per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));


			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 10);
			workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);

			PersistAndRemoveFromUnitOfWork(workflowControlSet);

			per1.WorkflowControlSet = workflowControlSet;
			per2.WorkflowControlSet = workflowControlSet;
			per3.WorkflowControlSet = workflowControlSet;

			PersistAndRemoveFromUnitOfWork(per1);
			PersistAndRemoveFromUnitOfWork(per2);
			PersistAndRemoveFromUnitOfWork(per3);

			var groupId = new Guid("B0E35119-4661-4A1B-8772-9B5E015B2564");

			persistReadModel(per1.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), groupId);

			var result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { groupId }, "");
			result.ToArray().Length.Should().Be.EqualTo(1);
		}



        [Test]
        public void ShouldSearchPersonWithCorrectNameFormat()
        {
            target = new PersonForScheduleFinder(CurrentUnitOfWork.Make());

            site = SiteFactory.CreateSimpleSite("d");

            PersistAndRemoveFromUnitOfWork(site);
            team = TeamFactory.CreateSimpleTeam();
            team.Site = site;
            team.SetDescription(new Description("sdf"));
            PersistAndRemoveFromUnitOfWork(team);

            contract1 = new Contract("contract1");
            contract2 = new Contract("contract2");

            PersistAndRemoveFromUnitOfWork(contract1);
            PersistAndRemoveFromUnitOfWork(contract2);

            per1 = PersonFactory.CreatePerson("roger", "kratz");
            per2 = PersonFactory.CreatePerson("z", "balog");
            per3 = PersonFactory.CreatePerson("a", "balog");

            per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract1), team));
            per2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));
            per3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(contract2), team));


            IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
            workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 10);
            workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
            workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);

			PersistAndRemoveFromUnitOfWork(workflowControlSet);

            per1.WorkflowControlSet = workflowControlSet;
            per2.WorkflowControlSet = workflowControlSet;
            per3.WorkflowControlSet = workflowControlSet;

            PersistAndRemoveFromUnitOfWork(per1);
            PersistAndRemoveFromUnitOfWork(per2);
            PersistAndRemoveFromUnitOfWork(per3);

            var groupId = new Guid("B0E35119-4661-4A1B-8772-9B5E015B2564");

            persistReadModel(per1.Id.GetValueOrDefault(), team.Id.GetValueOrDefault(), site.Id.GetValueOrDefault(), groupId);

            var result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { groupId }, "roger k", NameFormatSetting.FirstNameThenLastName);
            result.ToArray().Length.Should().Be.EqualTo(1);

            result = target.GetPersonFor(new DateOnly(2012, 2, 2), new List<Guid> { groupId }, "roger k", NameFormatSetting.LastNameThenFirstName);
            result.ToArray().Length.Should().Be.EqualTo(0);
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

		private Guid getBusinessUnitId()
		{
			return ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
		}

		private void persistReadModel(Guid personId, Guid teamId, Guid siteId, Guid groupId)
		{
			const string populateReadModelQuery = @"INSERT INTO [ReadModel].[GroupingReadOnly] 
                ([PersonId],[StartDate],[TeamId],[SiteId],[BusinessUnitId] ,[GroupId],[PageId])
			 VALUES (:personId,:startDate,:teamId,:siteId,:businessUnitId,:groupId,:pageId)";

			Session.CreateSQLQuery(populateReadModelQuery)
				.SetGuid("personId", personId)
				.SetDateTime("startDate", new DateTime(2012, 02, 02))
				.SetGuid("teamId", teamId)
				.SetGuid("siteId", siteId)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetGuid("groupId", groupId)
				.SetGuid("pageId", Guid.NewGuid())
				.ExecuteUpdate();
		}
	}
}
