using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	[TestFixture]
	[DatabaseTest]
	[Setting("ScheduleChangedMessagePackagingSendOnIdleTimeSeconds", 1)]
	[Setting("ScheduleChangedMessagePackagingSendOnIntervalSeconds", 1)]
	public class ScheduleChangedMessageTest
	{
		public FakeMessageSender MessageSender;

		public ICurrentDataSource DataSource;
		public ICurrentBusinessUnit BusinessUnit;

		public IJsonDeserializer Deserializer;
		public ICurrentUnitOfWorkFactory Uow;
		public IActivityRepository ActivityRepository;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakeTime Time;
		public Database Database;

		[Test]
		public void ShouldSendAggregatedScheduleChangeMessage()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);

				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 24));
				personAssignment.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));

				PersonAssignmentRepository.Add(personAssignment);

				uow.PersistAll();
			}
			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Should().Have.Count.EqualTo(1);
		}
		
		[Test]
		public void ShouldSendWithProperties()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);

				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 24));
				personAssignment.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));

				PersonAssignmentRepository.Add(personAssignment);

				uow.PersistAll();
			}
			Time.Passes("1".Seconds());

			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			message.DataSource.Should().Be(DataSource.CurrentName());
			message.BusinessUnitIdAsGuid().Should().Be(BusinessUnit.Current().Id.Value);
			message.StartDateAsDateTime().Should().Be("2015-06-24 8:00".Utc());
			message.EndDateAsDateTime().Should().Be("2015-06-24 17:00".Utc());
			message.DomainReferenceIdAsGuid().Should().Be(scenario.Id.Value);
			message.DomainQualifiedType.Should().Be(typeof (IScheduleChangedMessage).AssemblyQualifiedName);
			message.DomainUpdateType.Should().Be(DomainUpdateType.NotApplicable);
			message.BinaryData.Should().Contain(person.Id.ToString());
		}

		[Test]
		public void ShouldSendWithAllPersonIds()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);

				PersonRepository.Add(person1);
				PersonRepository.Add(person2);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment1 = new PersonAssignment(person1, scenario, new DateOnly(2015, 6, 24));
				personAssignment1.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));
				var personAssignment2 = new PersonAssignment(person2, scenario, new DateOnly(2015, 6, 24));
				personAssignment2.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));

				PersonAssignmentRepository.Add(personAssignment1);
				PersonAssignmentRepository.Add(personAssignment2);

				uow.PersistAll();
			}
			Time.Passes("1".Seconds());

			
			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			Deserializer.DeserializeObject<Guid[]>(message.BinaryData).Should().Have.SameValuesAs(new []{person1.Id.Value, person2.Id.Value});
		}

		[Test]
		public void ShouldNotSendDuplicatePersonIds()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);

				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment1 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 24));
				personAssignment1.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));
				var personAssignment2 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 25));
				personAssignment2.AddActivity(activity, new DateTimePeriod("2015-06-25 8:00".Utc(), "2015-06-25 17:00".Utc()));

				PersonAssignmentRepository.Add(personAssignment1);
				PersonAssignmentRepository.Add(personAssignment2);

				uow.PersistAll();
			}
			Time.Passes("1".Seconds());


			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			Deserializer.DeserializeObject<Guid[]>(message.BinaryData).Count().Should().Be(1);
		}

		[Test]
		public void ShouldSendWithEarliestStartDateOfPersonAssignments()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);

				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment1 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 24));
				personAssignment1.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));
				var personAssignment2 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 25));
				personAssignment2.AddActivity(activity, new DateTimePeriod("2015-06-25 8:00".Utc(), "2015-06-25 17:00".Utc()));

				PersonAssignmentRepository.Add(personAssignment1);
				PersonAssignmentRepository.Add(personAssignment2);

				uow.PersistAll();
			}
			Time.Passes("1".Seconds());

			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			message.StartDateAsDateTime().Should().Be("2015-06-24 8:00".Utc());
		}


		[Test]
		public void ShouldSendWithLatestEndDateOfPersonAssignments()
		{
			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);

				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment1 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 24));
				personAssignment1.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));
				var personAssignment2 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 25));
				personAssignment2.AddActivity(activity, new DateTimePeriod("2015-06-25 8:00".Utc(), "2015-06-25 17:00".Utc()));

				PersonAssignmentRepository.Add(personAssignment1);
				PersonAssignmentRepository.Add(personAssignment2);

				uow.PersistAll();
			}
			Time.Passes("1".Seconds());

			var message = MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single();
			message.EndDateAsDateTime().Should().Be("2015-06-25 17:00".Utc());
		}

		[Test]
		public void ShouldSendWithInitiatorId()
		{
			var InitiatorIdentifier = new FakeInitiatorIdentifier { InitiatorId = Guid.NewGuid() };

			var scenario = ScenarioFactory.CreateScenario(".", true, false);
			var person = PersonFactory.CreatePerson();

			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);

				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 24));
				personAssignment.AddActivity(activity, new DateTimePeriod("2015-06-24 8:00".Utc(), "2015-06-24 17:00".Utc()));

				PersonAssignmentRepository.Add(personAssignment);

				uow.PersistAll(InitiatorIdentifier);
			}
			Time.Passes("1".Seconds());

			MessageSender.NotificationsOfDomainType<IScheduleChangedMessage>().Single()
				.ModuleIdAsGuid().Should().Be(InitiatorIdentifier.InitiatorId);
		}
	}
}
