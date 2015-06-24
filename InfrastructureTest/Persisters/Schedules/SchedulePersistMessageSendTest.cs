using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	[InfrastructureTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class SchedulePersistMessageSendTest : ISetup
	{
		public IScheduleDictionaryPersister Target;
		public FakeMessageSender MessageSender;
		public ICurrentDataSource DataSource;
		public ICurrentBusinessUnit BusinessUnit;
		public ICurrentScenario CurrentScenario;

		public IJsonDeserializer Deserializer;

		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(SchedulePersistModule.ForOtherModules());
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
		}

		[Test]
		public void ShouldSendAggregatedScheduleChangeEvent()
		{
			var scenario = new Scenario(".").WithId();
			var person = PersonFactory.CreatePersonWithId();
			var period = new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc());
			var personAssignment = PersonAssignmentFactory.CreateEmptyAssignment(scenario, person, period);
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(scenario, period, personAssignment);
			
			Target.Persist(scheduleDictionary);

			MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Should().Have.Count.EqualTo(1);
		}
		
		[Test]
		public void ShouldSendAggregatedScheduleChangeEventWithProperties()
		{
			var scenario = new Scenario(".");
			var person = PersonFactory.CreatePerson();
			var period = new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc());
			var activity = new Activity(".");
			var shiftCategory = new ShiftCategory(".");
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ActivityRepository.Add(activity);
				ShiftCategoryRepository.Add(shiftCategory);
				PersonRepository.Add(person);
				ScenarioRepository.Add(scenario);
				uow.PersistAll();
			}
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, period, shiftCategory, scenario);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			scheduleDictionary.TakeSnapshot();
			scheduleDictionary.AddPersonAssignmentsWithoutSnapshot(personAssignment);
			
			Target.Persist(scheduleDictionary);
			
			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			message.DataSource.Should().Be(DataSource.CurrentName());
			message.BusinessUnitIdAsGuid().Should().Be(BusinessUnit.Current().Id.Value);
			message.StartDateAsDateTime().Should().Be("2015-06-24 8:00".Utc());
			message.EndDateAsDateTime().Should().Be("2015-06-24 17:00".Utc());
			message.DomainReferenceIdAsGuid().Should().Be(scenario.Id.Value);
			message.DomainUpdateType.Should().Be(DomainUpdateType.NotApplicable);
			message.BinaryData.Should().Contain(person.Id.ToString());
		}


		[Test]
		public void ShouldNotSendDuplicatePersonIds()
		{
			var scenario = new Scenario(".");
			var person = PersonFactory.CreatePerson();
			var activity = new Activity(".");
			var shiftCategory = new ShiftCategory(".");
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ActivityRepository.Add(activity);
				ShiftCategoryRepository.Add(shiftCategory);
				PersonRepository.Add(person);
				ScenarioRepository.Add(scenario);
				uow.PersistAll();
			}
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario,
				new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-25 17:00".Utc()));
			scheduleDictionary.TakeSnapshot();

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person,
				new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()), shiftCategory, scenario);
			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person,
				new DateTimePeriod("2015-06-25 8:00".Utc(), "2015-06-25 17:00".Utc()), shiftCategory, scenario);
			scheduleDictionary.AddPersonAssignmentsWithoutSnapshot(personAssignment, personAssignment2);

			Target.Persist(scheduleDictionary);

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			Deserializer.DeserializeObject<Guid[]>(message.BinaryData).Count().Should().Be(1);
		}

	}
}
