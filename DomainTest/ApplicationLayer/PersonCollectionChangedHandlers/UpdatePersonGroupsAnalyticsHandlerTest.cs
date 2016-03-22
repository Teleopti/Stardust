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
			@event.SetPersonIdCollection(new []{personId});

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
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] {});
			

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { skill.Id.GetValueOrDefault()}));
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
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { skill.Id.GetValueOrDefault() });


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
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


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
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

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
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

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
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

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
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

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
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
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new [] { ruleSetBag.Id.GetValueOrDefault() });

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldCreateNoteGroupIfNotExistingAndReuseGroupPageCodeIfExists()
		{
			var personPeriodId = Guid.NewGuid();
			var noteGroupPageId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.Note = "TestNote1";
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.FindGroupPageByGroupName(person.Note)).Return(null);
			_analyticsGroupPageRepository.Stub(r => r.FindGroupPageCodeByResourceKey("Note")).Return(noteGroupPageId);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.FindGroupPageByGroupName(person.Note));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.FindGroupPageCodeByResourceKey("Note"));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(Arg<AnalyticsGroupPage>.Matches(x => x.GroupName == person.Note && x.GroupPageName == "Note" && x.GroupPageNameResourceKey == "Note" && x.GroupPageCode == noteGroupPageId && x.GroupCode == person.Note.GenerateGuid())));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { person.Note.GenerateGuid() }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}

		[Test]
		public void ShouldCreateNoteGroupIfNotExistingAndGenerateNewGroupPageCodeIfNotExists()
		{
			var personPeriodId = Guid.NewGuid();
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
			person.Note = "TestNote1";
			person.SetId(Guid.NewGuid());
			person.PersonPeriodCollection.First().SetId(personPeriodId);
			var @event = new PersonCollectionChangedEvent();
			@event.SetPersonIdCollection(new[] { person.Id.GetValueOrDefault() });

			_personRepository.Stub(r => r.Get(person.Id.GetValueOrDefault())).Return(person);
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new Guid[] { });
			_analyticsGroupPageRepository.Stub(r => r.FindGroupPageByGroupName(person.Note)).Return(null);
			_analyticsGroupPageRepository.Stub(r => r.FindGroupPageCodeByResourceKey("Note")).Return(Guid.Empty);

			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.FindGroupPageByGroupName(person.Note));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.FindGroupPageCodeByResourceKey("Note"));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.AddGroupPage(Arg<AnalyticsGroupPage>.Matches(x => x.GroupName == person.Note && x.GroupPageName == "Note" && x.GroupPageNameResourceKey == "Note" && x.GroupPageCode != Guid.Empty && x.GroupCode == person.Note.GenerateGuid())));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));
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
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { existingGrouping });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBridgeGroupPagePerson(existingGrouping))
				.Return(new[] { Guid.NewGuid() });


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
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
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId))
				.Return(new[] { existingGrouping });
			_analyticsBridgeGroupPagePersonRepository.Stub(r => r.GetBridgeGroupPagePerson(existingGrouping))
				.Return(new Guid[] { });


			_target.Handle(@event);

			_personRepository.AssertWasCalled(r => r.Get(person.Id.GetValueOrDefault()));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBuiltInGroupPagesForPersonPeriod(personPeriodId));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonForPersonPeriod(personPeriodId, new[] { existingGrouping }));
			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.AddBridgeGroupPagePersonForPersonPeriod(personPeriodId, new Guid[] { }));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.GetBridgeGroupPagePerson(existingGrouping));
			_analyticsGroupPageRepository.AssertWasCalled(r => r.DeleteGroupPagesByGroupCodes(new Guid[] { existingGrouping }));

			_analyticsBridgeGroupPagePersonRepository.AssertWasCalled(r => r.DeleteBridgeGroupPagePersonExcludingPersonPeriods(person.Id.GetValueOrDefault(), new[] { personPeriodId }));
		}
	}
}