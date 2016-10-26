using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsScheduleMatchingPersonTest
	{
		private AnalyticsScheduleMatchingPerson Target;

	}

	[TestFixture]
	public class UpdatePersonGroupsAnalyticsHandlerTest
	{
		private AnalyticsPersonGroupsHandler _target;
		private IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private FakePersonRepository _personRepository;
		private IGroupPageRepository _groupPageRepository;
		private IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private Guid _businessUnitId;
		private Guid _personPeriodId;
		private IPerson _person;
		private AnalyticsPersonPeriod _analyticsPersonPeriod;

		[SetUp]
		public void Setup()
		{
			
			_personRepository = new FakePersonRepository();
			_analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			_analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();
			_groupPageRepository = MockRepository.GenerateMock<IGroupPageRepository>();
			_analyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository();

			_groupPageRepository.Stub(r => r.GetGroupPagesForPerson(Arg<Guid>.Is.Anything)).Return(new IGroupPage[] { });
			_target = new AnalyticsPersonGroupsHandler(_personRepository, _analyticsBridgeGroupPagePersonRepository, _analyticsGroupPageRepository, _groupPageRepository, _analyticsPersonPeriodRepository);
			_businessUnitId = Guid.NewGuid();
		}

		private void createPerson(IPerson person, bool createInAnalytics=true)
		{
			_personPeriodId = Guid.NewGuid();
			_person = person;
			_person.SetId(Guid.NewGuid());
			if (_person.PersonPeriodCollection.Any())
			{
				_person.PersonPeriodCollection.First().SetId(_personPeriodId);
				var bu = new Domain.Common.BusinessUnit("Test");
				bu.SetId(_businessUnitId);
				_person.PersonPeriodCollection.First().Team.Site = new Site("Test");
				_person.PersonPeriodCollection.First().Team.Site.SetBusinessUnit(bu);
			}
				
			_personRepository.Has(_person);
			if (createInAnalytics)
			{
				_analyticsPersonPeriod = new AnalyticsPersonPeriod { PersonCode = person.Id.GetValueOrDefault(), PersonPeriodCode = _personPeriodId, PersonId = 1, BusinessUnitCode = _businessUnitId};
				_analyticsPersonPeriodRepository.AddPersonPeriod(_analyticsPersonPeriod);
			}
		}

		private IContract createContract()
		{
			var contract = ContractFactory.CreateContract("TestContract1");
			contract.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			_person.PersonPeriodCollection.First().PersonContract = personContract;
			return contract;
		}

		private IContractSchedule createContractSchedule()
		{
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("TestContractSchedule1");
			contractSchedule.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.ContractSchedule = contractSchedule;
			_person.PersonPeriodCollection.First().PersonContract = personContract;
			return contractSchedule;
		}

		private IPartTimePercentage createPartTimePercentage()
		{
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("TestPartTimePercentage1");
			partTimePercentage.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.PartTimePercentage = partTimePercentage;
			_person.PersonPeriodCollection.First().PersonContract = personContract;
			return partTimePercentage;
		}

		private IRuleSetBag createRuleSetBag()
		{
			var ruleSetBag = new RuleSetBag();
			ruleSetBag.SetId(Guid.NewGuid());
			if (_person.PersonPeriodCollection.Any())
				_person.PersonPeriodCollection.First().RuleSetBag = ruleSetBag;
			return ruleSetBag;
		}

		private static IGroupPage createPersonGroup(out RootPersonGroup rootPersonGroup)
		{
			var groupPage = new GroupPage("CustomGroup");
			groupPage.SetId(Guid.NewGuid());
			rootPersonGroup = new RootPersonGroup("CustomRootGroup");
			rootPersonGroup.SetId(Guid.NewGuid());
			groupPage.AddRootPersonGroup(rootPersonGroup);
			return groupPage;
		}

		[Test]
		public void ShouldThrowWhenPersonPeriodMissingInAnalytics()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today), false);
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			Assert.Throws<PersonPeriodMissingInAnalyticsException>(() => _target.Handle(@event));
		}

		[Test]
		public void ShouldDeleteEverythingForNonExistingPerson()
		{
			var personId = Guid.NewGuid();
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new []{ personId });

			_target.Handle(@event);

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new int[] {}, _businessUnitId));
		}

		[Test]
		public void ShouldDeleteEverythingForPersonWithNoPeriods()
		{
			createPerson(new Person(), false);
			var @event = new AnalyticsPersonCollectionChangedEvent {LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });
			
			_target.Handle(@event);

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new int[] { }, _businessUnitId));
		}

		[Test]
		public void ShouldAddSkillAndCreateGroup()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] {skill}));

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill"};
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == skill.Id.GetValueOrDefault() && g.GroupName == skill.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { skill.Id.GetValueOrDefault() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldNotAddSkillIfAlreadyExisting()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] { skill }));

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { skill.Id.GetValueOrDefault() });


			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}


		[Test]
		public void ShouldNotAddContractIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { contract.Id.GetValueOrDefault() });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts" } });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldAddContractAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId))
				.Return(null);


			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == contract.Id.GetValueOrDefault() && g.GroupName == contract.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { contract.Id.GetValueOrDefault() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldAddContractAndCreateGroupPage()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new AnalyticsGroupPage[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId))
				.Return(null);


			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == contract.Id.GetValueOrDefault() && g.GroupName == contract.Description.Name &&
						g.GroupPageName == Resources.Contracts && g.GroupPageCode != Guid.Empty &&
						g.GroupPageNameResourceKey == "Contracts")));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { contract.Id.GetValueOrDefault() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldNotAddContractScheduleIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contractSchedule = createContractSchedule();
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { contractSchedule.Id.GetValueOrDefault() });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldAddContractScheduleAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contractSchedule = createContractSchedule();

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == contractSchedule.Id.GetValueOrDefault() && g.GroupName == contractSchedule.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { contractSchedule.Id.GetValueOrDefault() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldNotAddPartTimePercentageIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var partTimePercentage = createPartTimePercentage();
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { partTimePercentage.Id.GetValueOrDefault() });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}



		[Test]
		public void ShouldAddPartTimePercentageAndCreateGroup()
		{
			var personPeriodId = Guid.NewGuid();
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var partTimePercentage = createPartTimePercentage();

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == partTimePercentage.Id.GetValueOrDefault() && g.GroupName == partTimePercentage.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { partTimePercentage.Id.GetValueOrDefault() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldNotAddRuleSetBagIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var ruleSetBag = createRuleSetBag();
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { ruleSetBag.Id.GetValueOrDefault() });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}



		[Test]
		public void ShouldAddRuleSetBagAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var ruleSetBag = createRuleSetBag();
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == ruleSetBag.Id.GetValueOrDefault() && g.GroupName == ruleSetBag.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { ruleSetBag.Id.GetValueOrDefault() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldNotAddNoteIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			_person.Note = "TestNote1";

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { _person.Note.GenerateGuid() });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldAddNoteAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			_person.Note = "TestNote1";

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });

			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == _person.Note.GenerateGuid() && g.GroupName == _person.Note &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { _person.Note.GenerateGuid() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldAddCustomGroupAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));

			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });
			RootPersonGroup rootPersonGroup;
			var groupPage = createPersonGroup(out rootPersonGroup);

			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new AnalyticsGroupPage[] { });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new Guid[] { });
			

			_groupPageRepository.Stub(r => r.GetGroupPagesForPerson(_person.Id.GetValueOrDefault()))
				.Return(new[] { groupPage }).Repeat.Any();

			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPageIfNotExisting(
					Arg<AnalyticsGroup>.Matches(
						g =>
							g.GroupCode == rootPersonGroup.Id.GetValueOrDefault() && g.GroupName == rootPersonGroup.Description.Name &&
							g.GroupPageName == groupPage.Description.Name && g.GroupPageCode == groupPage.Id.GetValueOrDefault() &&
							g.GroupPageNameResourceKey == groupPage.DescriptionKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { rootPersonGroup.Id.GetValueOrDefault() }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}



		[Test]
		public void ShouldNotAddCustomGroupIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			RootPersonGroup rootPersonGroup;
			var groupPage = createPersonGroup(out rootPersonGroup);

			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new AnalyticsGroupPage[] { });
			_groupPageRepository.Stub(r => r.GetGroupPagesForPerson(_person.Id.GetValueOrDefault()))
				.Return(new[] { groupPage }).Repeat.Any();
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { rootPersonGroup.Id.GetValueOrDefault() });


			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldRemoveExistingGroupings()
		{
			var existingGrouping = Guid.NewGuid();
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				.Return(new AnalyticsGroupPage[] { });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { existingGrouping });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBridgeGroupPagePerson(existingGrouping, _businessUnitId))
				.Return(new[] { Guid.NewGuid() });


			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { existingGrouping }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBridgeGroupPagePerson(existingGrouping, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.DeleteGroupPagesByGroupCodes(new Guid[] { }, _businessUnitId));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}

		[Test]
		public void ShouldRemoveExistingGroupingsAndGroupIfNoGroupingsLeft()
		{
			var existingGrouping = Guid.NewGuid();
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var @event = new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId };
			@event.SetPersonIdCollection(new[] { _person.Id.GetValueOrDefault() });

			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase(_businessUnitId))
				 .Return(new AnalyticsGroupPage[] { });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId))
				.Return(new[] { existingGrouping });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBridgeGroupPagePerson(existingGrouping, _businessUnitId))
				.Return(new Guid[] { });


			_target.Handle(@event);

			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase(_businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetGroupPagesForPersonPeriod(_analyticsPersonPeriod.PersonId, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new[] { existingGrouping }, _businessUnitId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(_analyticsPersonPeriod.PersonId, new Guid[] { }, _businessUnitId));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBridgeGroupPagePerson(existingGrouping, _businessUnitId));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.DeleteGroupPagesByGroupCodes(new[] { existingGrouping }, _businessUnitId));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(_person.Id.GetValueOrDefault(), new[] { _analyticsPersonPeriod.PersonId }, _businessUnitId));
		}
	}
}