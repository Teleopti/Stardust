using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsPersonGroupsHandlerTest
	{
		private AnalyticsPersonGroupsHandler _target;
		private IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private FakeAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
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
			
			_personRepository = new FakePersonRepositoryLegacy2();
			_analyticsGroupPageRepository = new FakeAnalyticsGroupPageRepository();
			_analyticsBridgeGroupPagePersonRepository = new FakeAnalyticsBridgeGroupPagePersonRepository();
			_groupPageRepository = new FakeGroupPageRepository();
			_analyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository();

			_target = new AnalyticsPersonGroupsHandler(_personRepository, _analyticsBridgeGroupPagePersonRepository, _analyticsGroupPageRepository, _groupPageRepository, _analyticsPersonPeriodRepository);
			_businessUnitId = Guid.NewGuid();
		}

		private void createPerson(IPerson person, bool createInAnalytics=true)
		{
			_personPeriodId = Guid.NewGuid();
			_person = person.WithId(Guid.NewGuid());
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

		private static IGroupPage createPersonGroup(out RootPersonGroup rootPersonGroup, IPerson person)
		{
			var groupPage = new GroupPage("CustomGroup");
			groupPage.SetId(Guid.NewGuid());
			rootPersonGroup = new RootPersonGroup("CustomRootGroup");
			rootPersonGroup.SetId(Guid.NewGuid());
			groupPage.AddRootPersonGroup(rootPersonGroup);
			rootPersonGroup.AddPerson(person);
			return groupPage;
		}

		[Test]
		public void ShouldThrowWhenPersonPeriodMissingInAnalytics()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today), false);

			Assert.Throws<PersonPeriodMissingInAnalyticsException>(
				() =>
					_target.Handle(new AnalyticsPersonCollectionChangedEvent
					{
						LogOnBusinessUnitId = _businessUnitId,
						PersonIdCollection = {_person.Id.GetValueOrDefault()}
					}));
		}

		[Test]
		public void ShouldDeleteEverythingForNonExistingPerson()
		{
			var personId = Guid.NewGuid();
			_analyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = Guid.NewGuid(),
					GroupPageCode = Guid.NewGuid(),
					PersonId = 1,
					PersonCode = personId
				})
				.WithPersonMapping(personId, 1);
			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { personId } });

			_analyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteEverythingForPersonWithNoPeriods()
		{
			createPerson(new Person(), false);
			_analyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = Guid.NewGuid(),
					GroupPageCode = Guid.NewGuid(),
					PersonId = 1,
					PersonCode = _person.Id.GetValueOrDefault()
				})
				.WithPersonMapping(_person.Id.GetValueOrDefault(), 1);


			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddSkillAndCreateGroup()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] {skill}));

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill", BusinessUnitCode = _businessUnitId};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(skill.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(skill.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotAddSkillIfAlreadyExisting()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] { skill }));

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill", BusinessUnitCode = _businessUnitId, GroupCode = skill.Id.GetValueOrDefault()};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = skill.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			_analyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(skill.Id.Value, _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}


		[Test]
		public void ShouldNotAddContractIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts", BusinessUnitCode = _businessUnitId, GroupCode = contract.Id.GetValueOrDefault()};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = contract.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			_analyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddContractAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts", BusinessUnitCode = _businessUnitId};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(contract.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(contract.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldAddContractAndCreateGroupPage()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Not.Be.EqualTo(Guid.Empty);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(Resources.Contracts);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo("Contracts");
			addedGroupPage.GroupName.Should().Be.EqualTo(contract.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(contract.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotAddContractScheduleIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contractSchedule = createContractSchedule();

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule", BusinessUnitCode = _businessUnitId, GroupCode = contractSchedule.Id.GetValueOrDefault() };
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = contractSchedule.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			_analyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddContractScheduleAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contractSchedule = createContractSchedule();

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule", BusinessUnitCode = _businessUnitId};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(contractSchedule.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(contractSchedule.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotAddPartTimePercentageIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var partTimePercentage = createPartTimePercentage();

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages", BusinessUnitCode = _businessUnitId, GroupCode = partTimePercentage.Id.GetValueOrDefault()};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = partTimePercentage.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			_analyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddPartTimePercentageAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var partTimePercentage = createPartTimePercentage();

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages", BusinessUnitCode = _businessUnitId};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(partTimePercentage.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(partTimePercentage.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotAddRuleSetBagIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var ruleSetBag = createRuleSetBag();

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag", BusinessUnitCode = _businessUnitId, GroupCode = ruleSetBag.Id.GetValueOrDefault()};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = ruleSetBag.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			_analyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddRuleSetBagAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var ruleSetBag = createRuleSetBag();

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag", BusinessUnitCode = _businessUnitId};
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(ruleSetBag.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(ruleSetBag.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotAddNoteIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			_person.Note = "TestNote1";

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note", BusinessUnitCode = _businessUnitId, GroupCode = _person.Note.GenerateGuid() };
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = _person.Note.GenerateGuid(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};

			_analyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(_person.Note.GenerateGuid(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddNoteAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			_person.Note = "TestNote1";

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note", BusinessUnitCode = _businessUnitId };
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(_person.Note.GenerateGuid(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(_person.Note);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(_person.Note.GenerateGuid());
		}

		[Test]
		public void ShouldAddCustomGroupAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			RootPersonGroup rootPersonGroup;
			var groupPage = createPersonGroup(out rootPersonGroup, _person);

			_analyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_groupPageRepository.Add(groupPage);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPage.Id.GetValueOrDefault());
			addedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = _analyticsBridgeGroupPagePersonRepository.Bridges.Single();
			bridge.PersonId.Should().Be.EqualTo(_analyticsPersonPeriod.PersonId);
			bridge.PersonCode.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			bridge.BusinessUnitCode.Should().Be.EqualTo(_businessUnitId);
			bridge.GroupCode.Should().Be.EqualTo(rootPersonGroup.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldNotAddCustomGroupIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			
			RootPersonGroup rootPersonGroup;
			var groupPage = createPersonGroup(out rootPersonGroup, _person);

			_groupPageRepository.Add(groupPage);

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = groupPage.Id.GetValueOrDefault(), GroupPageName = groupPage.Description.Name, GroupPageNameResourceKey = groupPage.DescriptionKey, BusinessUnitCode = _businessUnitId , GroupCode = rootPersonGroup.Id.GetValueOrDefault() };
			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			_analyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			_analyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldRemoveExistingGroupings()
		{
			var existingGrouping = Guid.NewGuid();
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));

			_analyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = existingGrouping,
					PersonId = _analyticsPersonPeriod.PersonId,
					PersonCode = _person.Id.GetValueOrDefault(),
					GroupPageCode = Guid.NewGuid()
				})
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveExistingGroupingsAndGroupIfNoGroupingsLeft()
		{
			var existingGrouping = Guid.NewGuid();
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));

			_analyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = existingGrouping,
					PersonId = _analyticsPersonPeriod.PersonId,
					PersonCode = _person.Id.GetValueOrDefault(),
					GroupPageCode = Guid.NewGuid()
				})
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			_analyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup {GroupCode = existingGrouping, BusinessUnitCode = _businessUnitId});

			_target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			_analyticsGroupPageRepository.GetGroupPageByGroupCode(existingGrouping, _businessUnitId).Should().Be.Null();
			_analyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}
	}
}