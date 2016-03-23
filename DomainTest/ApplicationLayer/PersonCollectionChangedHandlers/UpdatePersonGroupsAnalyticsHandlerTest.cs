using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers
{
	[TestFixture]
	public class UpdatePersonGroupsAnalyticsHandlerTest
	{
		private UpdatePersonGroupsAnalyticsHandler _target;
		private IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private IAnalyticsBridgeGroupPagePersonRepository _analyticsBridgeGroupPagePersonRepository;
		private IPersonRepository _personRepository;

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_analyticsGroupPageRepository = MockRepository.GenerateMock<IAnalyticsGroupPageRepository>();
			_analyticsBridgeGroupPagePersonRepository = MockRepository.GenerateMock<IAnalyticsBridgeGroupPagePersonRepository>();

			_target = new UpdatePersonGroupsAnalyticsHandler(_personRepository, _analyticsBridgeGroupPagePersonRepository, _analyticsGroupPageRepository);
		}

		[Test]
		public void ShouldDeleteEverythingForNonExistingPerson()
		{
			var personId = Guid.NewGuid();
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new []{ personId });

			_personRepository.Stub(r => r.Get(personId)).Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(personId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(personId, new Guid[] {}));
		}

		[Test]
		public void ShouldDeleteEverythingForPersonWithNoPeriods()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });
			
			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new Guid[] { }));
		}

		[Test]
		public void ShouldAddSkill()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] { skill });
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] {new AnalyticsGroupPage {GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill" } });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] {});
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault()))
				.Return(new AnalyticsGroup());

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { skill.Id.GetValueOrDefault()}));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddSkillAndCreateGroup()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] { skill });
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill"};
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == skill.Id.GetValueOrDefault() && g.GroupName == skill.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { skill.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddSkillAndCreateGroupPage()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] { skill });
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new AnalyticsGroupPage[] { });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(skill.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == skill.Id.GetValueOrDefault() && g.GroupName == skill.Name &&
						g.GroupPageName == "Skill" && g.GroupPageCode != Guid.Empty &&
						g.GroupPageNameResourceKey == Resources.Skill)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { skill.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldNotAddSkillIfAlreadyExisting()
		{
			var skill = SkillFactory.CreateSkillWithId("TestSkill1");
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today, new[] { skill });
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Skill, GroupPageNameResourceKey = "Skill" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { skill.Id.GetValueOrDefault() });


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddContract()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contract = ContractFactory.CreateContract("TestContract1");
			contract.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts" } });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault()))
				.Return(new AnalyticsGroup());


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { contract.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldNotAddContractIfAlreadyExisting()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contract = ContractFactory.CreateContract("TestContract1");
			contract.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { contract.Id.GetValueOrDefault() });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts" } });

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddContractAndCreateGroup()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contract = ContractFactory.CreateContract("TestContract1");
			contract.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Contracts, GroupPageNameResourceKey = "Contracts" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault()))
				.Return(null);


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == contract.Id.GetValueOrDefault() && g.GroupName == contract.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { contract.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddContractAndCreateGroupPage()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contract = ContractFactory.CreateContract("TestContract1");
			contract.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract(contract);
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new AnalyticsGroupPage[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault()))
				.Return(null);


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(contract.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == contract.Id.GetValueOrDefault() && g.GroupName == contract.Description.Name &&
						g.GroupPageName == Resources.Contracts && g.GroupPageCode != Guid.Empty &&
						g.GroupPageNameResourceKey == "Contracts")));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { contract.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}
		[Test]
		public void ShouldAddContractSchedule()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("TestContractSchedule1");
			contractSchedule.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.ContractSchedule = contractSchedule;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule" } });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault()))
				.Return(new AnalyticsGroup());

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { contractSchedule.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldNotAddContractScheduleIfAlreadyExisting()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("TestContractSchedule1");
			contractSchedule.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.ContractSchedule = contractSchedule;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { contractSchedule.Id.GetValueOrDefault() });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddContractScheduleAndCreateGroup()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("TestContractSchedule1");
			contractSchedule.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.ContractSchedule = contractSchedule;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.ContractSchedule, GroupPageNameResourceKey = "ContractSchedule" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == contractSchedule.Id.GetValueOrDefault() && g.GroupName == contractSchedule.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { contractSchedule.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddContractScheduleAndCreateGroupPage()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var contractSchedule = ContractScheduleFactory.CreateContractSchedule("TestContractSchedule1");
			contractSchedule.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.ContractSchedule = contractSchedule;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new AnalyticsGroupPage[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(contractSchedule.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == contractSchedule.Id.GetValueOrDefault() && g.GroupName == contractSchedule.Description.Name &&
						g.GroupPageName == Resources.ContractSchedule && g.GroupPageCode != Guid.Empty &&
						g.GroupPageNameResourceKey == "ContractSchedule")));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { contractSchedule.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddPartTimePercentage()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("TestPartTimePercentage1");
			partTimePercentage.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.PartTimePercentage = partTimePercentage;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages" } });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault()))
				.Return(new AnalyticsGroup());

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { partTimePercentage.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldNotAddPartTimePercentageIfAlreadyExisting()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("TestPartTimePercentage1");
			partTimePercentage.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.PartTimePercentage = partTimePercentage;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { partTimePercentage.Id.GetValueOrDefault() });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddPartTimePercentageAndCreateGroup()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("TestPartTimePercentage1");
			partTimePercentage.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.PartTimePercentage = partTimePercentage;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.PartTimepercentages, GroupPageNameResourceKey = "PartTimepercentages" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == partTimePercentage.Id.GetValueOrDefault() && g.GroupName == partTimePercentage.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { partTimePercentage.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddPartTimePercentageAndCreateGroupPage()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("TestPartTimePercentage1");
			partTimePercentage.SetId(Guid.NewGuid());
			var personContract = PersonContractFactory.CreatePersonContract();
			personContract.PartTimePercentage = partTimePercentage;
			person.PersonPeriodCollection.First().PersonContract = personContract;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new AnalyticsGroupPage[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(partTimePercentage.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == partTimePercentage.Id.GetValueOrDefault() && g.GroupName == partTimePercentage.Description.Name &&
						g.GroupPageName == Resources.PartTimepercentages && g.GroupPageCode != Guid.Empty &&
						g.GroupPageNameResourceKey == "PartTimepercentages")));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { partTimePercentage.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddRuleSetBag()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var ruleSetBag = new RuleSetBag();
			ruleSetBag.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().RuleSetBag = ruleSetBag;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault()))
				.Return(new AnalyticsGroup());

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { ruleSetBag.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldNotAddRuleSetBagIfAlreadyExisting()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var ruleSetBag = new RuleSetBag();
			ruleSetBag.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().RuleSetBag = ruleSetBag;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new [] { ruleSetBag.Id.GetValueOrDefault() });

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddRuleSetBagAndCreateGroup()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var ruleSetBag = new RuleSetBag();
			ruleSetBag.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().RuleSetBag = ruleSetBag;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.RuleSetBag, GroupPageNameResourceKey = "RuleSetBag" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == ruleSetBag.Id.GetValueOrDefault() && g.GroupName == ruleSetBag.Description.Name &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { ruleSetBag.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddRuleSetBagAndCreateGroupPage()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var ruleSetBag = new RuleSetBag();
			ruleSetBag.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().RuleSetBag = ruleSetBag;
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new AnalyticsGroupPage[] { });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(ruleSetBag.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == ruleSetBag.Id.GetValueOrDefault() && g.GroupName == ruleSetBag.Description.Name &&
						g.GroupPageName == Resources.RuleSetBag && g.GroupPageCode != Guid.Empty &&
						g.GroupPageNameResourceKey == "RuleSetBag")));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { ruleSetBag.Id.GetValueOrDefault() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddNote()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			person.Note = "TestNote1";

			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });

			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(person.Note.GenerateGuid()))
				.Return(new AnalyticsGroup());

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(person.Note.GenerateGuid()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { person.Note.GenerateGuid() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldNotAddNoteIfAlreadyExisting()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			person.Note = "TestNote1";

			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { person.Note.GenerateGuid() });

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddNoteAndCreateGroup()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			person.Note = "TestNote1";

			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });

			var analyticsGroupPage = new AnalyticsGroupPage { GroupPageCode = Guid.NewGuid(), GroupPageName = Resources.Note, GroupPageNameResourceKey = "Note" };
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new[] { analyticsGroupPage });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(person.Note.GenerateGuid()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(person.Note.GenerateGuid()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == person.Note.GenerateGuid() && g.GroupName == person.Note &&
						g.GroupPageName == analyticsGroupPage.GroupPageName && g.GroupPageCode == analyticsGroupPage.GroupPageCode &&
						g.GroupPageNameResourceKey == analyticsGroupPage.GroupPageNameResourceKey)));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { person.Note.GenerateGuid() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldAddNoteAndCreateGroupPage()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			person.Note = "TestNote1";

			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });

			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new AnalyticsGroupPage[] { });

			_analyticsGroupPageRepository.Stub(r => r.GetGroupPageByGroupCode(person.Note.GenerateGuid()))
				.Return(null);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetGroupPageByGroupCode(person.Note.GenerateGuid()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(
				Arg<AnalyticsGroup>.Matches(
					g =>
						g.GroupCode == person.Note.GenerateGuid() && g.GroupName == person.Note &&
						g.GroupPageName == Resources.Note && g.GroupPageCode != Guid.Empty &&
						g.GroupPageNameResourceKey == "Note")));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { person.Note.GenerateGuid() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldRemoveExistingGroupings()
		{
			var personPeriodId = Guid.NewGuid();
			var existingGrouping = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				.Return(new AnalyticsGroupPage[] { });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { existingGrouping });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBridgeGroupPagePerson(existingGrouping))
				.Return(new[] { Guid.NewGuid() });


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { existingGrouping }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBridgeGroupPagePerson(existingGrouping));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.DeleteGroupPagesByGroupCodes(new Guid[] { }));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldRemoveExistingGroupingsAndGroupIfNoGroupingsLeft()
		{
			var personPeriodId = Guid.NewGuid();
			var existingGrouping = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsGroupPageRepository.Stub(r => r.GetBuildInGroupPageBase())
				 .Return(new AnalyticsGroupPage[] { });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { existingGrouping });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBridgeGroupPagePerson(existingGrouping))
				.Return(new Guid[] { });


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.GetBuildInGroupPageBase());
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { existingGrouping }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBridgeGroupPagePerson(existingGrouping));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.DeleteGroupPagesByGroupCodes(new[] { existingGrouping }));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}
	}
}