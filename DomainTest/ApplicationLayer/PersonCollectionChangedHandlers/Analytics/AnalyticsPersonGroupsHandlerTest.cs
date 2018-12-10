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
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsPersonGroupsHandlerTest : IExtendSystem
	{
		public AnalyticsPersonGroupsHandler Target;
		public IAnalyticsGroupPageRepository AnalyticsGroupPageRepository;
		public FakeAnalyticsBridgeGroupPagePersonRepository AnalyticsBridgeGroupPagePersonRepository;
		public FakePersonRepository PersonRepository;
		public IGroupPageRepository GroupPageRepository;
		public IAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;

		private readonly Guid _businessUnitId = Guid.NewGuid();
		private Guid _personPeriodId;
		private IPerson _person;
		private AnalyticsPersonPeriod _analyticsPersonPeriod;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsPersonGroupsHandler>();
		}

		private void createPerson(IPerson person, bool createInAnalytics=true)
		{
			_personPeriodId = Guid.NewGuid();
			_person = person.WithId(Guid.NewGuid());
			if (_person.PersonPeriodCollection.Any())
			{
				_person.PersonPeriodCollection.First().SetId(_personPeriodId);
				var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("Test").WithId(_businessUnitId);
				_person.PersonPeriodCollection.First().Team.Site = new Site("Test");
				_person.PersonPeriodCollection.First().Team.Site.SetBusinessUnit(bu);
			}
				
			PersonRepository.Has(_person);
			if (createInAnalytics)
			{
				_analyticsPersonPeriod = new AnalyticsPersonPeriod { PersonCode = person.Id.GetValueOrDefault(), PersonPeriodCode = _personPeriodId, PersonId = 1, BusinessUnitCode = _businessUnitId};
				AnalyticsPersonPeriodRepository.AddOrUpdatePersonPeriod(_analyticsPersonPeriod);
			}
		}

		private IContract createContract()
		{
			var contract = ContractFactory.CreateContract("TestContract1").WithId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			_person.PersonPeriodCollection.First().PersonContract = personContract;
			return contract;
		}

		private IContractSchedule createContractSchedule()
		{
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("TestContractSchedule1").WithId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.ContractSchedule = contractSchedule;
			_person.PersonPeriodCollection.First().PersonContract = personContract;
			return contractSchedule;
		}

		private IPartTimePercentage createPartTimePercentage()
		{
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("TestPartTimePercentage1").WithId(Guid.NewGuid());
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
			var groupPage = new GroupPage("CustomGroup").WithId(Guid.NewGuid());
			rootPersonGroup = new RootPersonGroup("CustomRootGroup").WithId(Guid.NewGuid());
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
					Target.Handle(new AnalyticsPersonCollectionChangedEvent
					{
						LogOnBusinessUnitId = _businessUnitId,
						PersonIdCollection = {_person.Id.GetValueOrDefault()}
					}));
		}

		[Test]
		public void ShouldDeleteEverythingForNonExistingPerson()
		{
			var personId = Guid.NewGuid();
			AnalyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = Guid.NewGuid(),
					GroupPageCode = Guid.NewGuid(),
					PersonId = 1,
					PersonCode = personId
				})
				.WithPersonMapping(personId, 1);
			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { personId } });

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldDeleteEverythingForPersonWithNoPeriods()
		{
			createPerson(new Person(), false);
			AnalyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = Guid.NewGuid(),
					GroupPageCode = Guid.NewGuid(),
					PersonId = 1,
					PersonCode = _person.Id.GetValueOrDefault()
				})
				.WithPersonMapping(_person.Id.GetValueOrDefault(), 1);


			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldAddSkillAndCreateGroup()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] {skill}));

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill", BusinessUnitCode = _businessUnitId};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(skill.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = skill.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			AnalyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(skill.Id.Value, _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}


		[Test]
		public void ShouldNotAddContractIfAlreadyExisting()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts", BusinessUnitCode = _businessUnitId, GroupCode = contract.Id.GetValueOrDefault()};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = contract.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			AnalyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddContractAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contract = createContract();

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts", BusinessUnitCode = _businessUnitId};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(contract.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Not.Be.EqualTo(Guid.Empty);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(Resources.Contracts);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo("Contracts");
			addedGroupPage.GroupName.Should().Be.EqualTo(contract.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = contractSchedule.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			AnalyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddContractScheduleAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var contractSchedule = createContractSchedule();

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule", BusinessUnitCode = _businessUnitId};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(contractSchedule.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = partTimePercentage.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			AnalyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddPartTimePercentageAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var partTimePercentage = createPartTimePercentage();

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages", BusinessUnitCode = _businessUnitId};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(partTimePercentage.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = ruleSetBag.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			AnalyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddRuleSetBagAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			var ruleSetBag = createRuleSetBag();

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);
			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag", BusinessUnitCode = _businessUnitId};
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(ruleSetBag.Description.Name);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = _person.Note.GenerateGuid(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};

			AnalyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(_person.Note.GenerateGuid(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldAddNoteAndCreateGroup()
		{
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));
			_person.Note = "TestNote1";

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note", BusinessUnitCode = _businessUnitId };
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(_person.Note.GenerateGuid(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(analyticsGroupPage.GroupPageCode);
			addedGroupPage.GroupPageName.Should().Be.EqualTo(analyticsGroupPage.GroupPageName);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(analyticsGroupPage.GroupPageNameResourceKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(_person.Note);
			addedGroupPage.GroupIsCustom.Should().Be.False();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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

			AnalyticsBridgeGroupPagePersonRepository.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			GroupPageRepository.Add(groupPage);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			var addedGroupPage = AnalyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId);
			addedGroupPage.Should().Not.Be.Null();
			addedGroupPage.GroupPageCode.Should().Be.EqualTo(groupPage.Id.GetValueOrDefault());
			addedGroupPage.GroupPageName.Should().Be.EqualTo(groupPage.Description.Name);
			addedGroupPage.GroupPageNameResourceKey.Should().Be.EqualTo(groupPage.DescriptionKey);
			addedGroupPage.GroupName.Should().Be.EqualTo(rootPersonGroup.Name);
			addedGroupPage.GroupIsCustom.Should().Be.True();

			var bridge = AnalyticsBridgeGroupPagePersonRepository.Bridges.Single();
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

			GroupPageRepository.Add(groupPage);

			var analyticsGroupPage = new AnalyticsGroup { GroupPageCode = groupPage.Id.GetValueOrDefault(), GroupPageName = groupPage.Description.Name, GroupPageNameResourceKey = groupPage.DescriptionKey, BusinessUnitCode = _businessUnitId , GroupCode = rootPersonGroup.Id.GetValueOrDefault() };
			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(analyticsGroupPage);

			var analyticsBridge = new AnalyticsBridgeGroupPagePerson
			{
				BusinessUnitCode = _businessUnitId,
				GroupCode = rootPersonGroup.Id.GetValueOrDefault(),
				PersonId = _analyticsPersonPeriod.PersonId,
				PersonCode = _person.Id.GetValueOrDefault(),
				GroupPageCode = analyticsGroupPage.GroupPageCode
			};
			AnalyticsBridgeGroupPagePersonRepository
				.Has(analyticsBridge)
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(rootPersonGroup.Id.GetValueOrDefault(), _businessUnitId)
				.Should()
				.Be.SameInstanceAs(analyticsGroupPage);

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Single().Should().Be.SameInstanceAs(analyticsBridge);
		}

		[Test]
		public void ShouldRemoveExistingGroupings()
		{
			var existingGrouping = Guid.NewGuid();
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));

			AnalyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = existingGrouping,
					PersonId = _analyticsPersonPeriod.PersonId,
					PersonCode = _person.Id.GetValueOrDefault(),
					GroupPageCode = Guid.NewGuid()
				})
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}

		[Test]
		public void ShouldRemoveExistingGroupingsAndGroupIfNoGroupingsLeft()
		{
			var existingGrouping = Guid.NewGuid();
			createPerson(PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today));

			AnalyticsBridgeGroupPagePersonRepository
				.Has(new AnalyticsBridgeGroupPagePerson
				{
					BusinessUnitCode = _businessUnitId,
					GroupCode = existingGrouping,
					PersonId = _analyticsPersonPeriod.PersonId,
					PersonCode = _person.Id.GetValueOrDefault(),
					GroupPageCode = Guid.NewGuid()
				})
				.WithPersonMapping(_person.Id.GetValueOrDefault(), _analyticsPersonPeriod.PersonId);

			AnalyticsGroupPageRepository.AddGroupPageIfNotExisting(new AnalyticsGroup {GroupCode = existingGrouping, BusinessUnitCode = _businessUnitId});

			Target.Handle(new AnalyticsPersonCollectionChangedEvent { LogOnBusinessUnitId = _businessUnitId, PersonIdCollection = { _person.Id.GetValueOrDefault() } });

			AnalyticsGroupPageRepository.GetGroupPageByGroupCode(existingGrouping, _businessUnitId).Should().Be.Null();
			AnalyticsBridgeGroupPagePersonRepository.Bridges.Should().Be.Empty();
		}
	}
}