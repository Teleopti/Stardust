using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.MessageSenders
{
	[TestFixture]
	[PrincipalAndStateTest]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	public class NotifyMessageBrokerTest
	{
		public ICurrentUnitOfWorkFactory Uow;
		public FakeMessageSender MessageSender;
		public ICurrentDataSource DataSource;
		public ICurrentBusinessUnit BusinessUnit;
		
		public IPersonRepository PersonRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IActivityRepository ActivityRepository;
		public IScenarioRepository ScenarioRepository;
		public IJsonDeserializer JsonDeserializer;

		[Test]
		public void ShouldSendAggregatedScheduleChangeMessage()
		{
			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				var scenario = ScenarioFactory.CreateScenario(".", true, false);
				ScenarioRepository.Add(scenario);

				var person = PersonFactory.CreatePerson();
				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 15));
				personAssignment.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));

				PersonAssignmentRepository.Add(personAssignment);

				uow.PersistAll();
			}

			MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSendAggregatedScheduleChangeMessageWithProperties()
		{
			IScenario scenario;
			IPerson person;
			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				scenario = ScenarioFactory.CreateScenario(".", true, false);
				ScenarioRepository.Add(scenario);

				person = PersonFactory.CreatePerson();
				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 15));
				personAssignment.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));

				PersonAssignmentRepository.Add(personAssignment);

				uow.PersistAll();
			}

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			message.DataSource.Should().Be(DataSource.CurrentName());
			message.BusinessUnitIdAsGuid().Should().Be(BusinessUnit.Current().Id.Value);
			message.StartDateAsDateTime().Should().Be("2015-06-15 8:00".Utc());
			message.EndDateAsDateTime().Should().Be("2015-06-15 17:00".Utc());
			message.DomainReferenceIdAsGuid().Should().Be(scenario.Id.Value);
			message.DomainUpdateType.Should().Be(DomainUpdateType.NotApplicable);
			message.BinaryData.Should().Contain(person.Id.ToString());
		}

		[Test]
		public void ShouldSendAggregatedScheduleChangeMessageForTwoPersons()
		{
			IPerson person1;
			IPerson person2;
			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				var scenario = ScenarioFactory.CreateScenario(".", true, false);
				ScenarioRepository.Add(scenario);

				person1 = PersonFactory.CreatePerson();
				PersonRepository.Add(person1);
				person2 = PersonFactory.CreatePerson();
				PersonRepository.Add(person2);
			
				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment1 = new PersonAssignment(person1, scenario, new DateOnly(2015, 6, 15));
				personAssignment1.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));
				PersonAssignmentRepository.Add(personAssignment1);
				var personAssignment2 = new PersonAssignment(person2, scenario, new DateOnly(2015, 6, 16));
				personAssignment2.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));
				PersonAssignmentRepository.Add(personAssignment2);

				uow.PersistAll();
			}

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			var personIds = JsonDeserializer.DeserializeObject<Guid[]>(message.BinaryData);
			personIds.Should().Have.SameValuesAs(new[] {person1.Id.Value, person2.Id.Value});
		}

		[Test]
		public void ShouldSendAggregatedScheduleChangeMessageForTwoDays()
		{
			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				var scenario = ScenarioFactory.CreateScenario(".", true, false);
				ScenarioRepository.Add(scenario);

				var person = PersonFactory.CreatePerson();
				PersonRepository.Add(person);
			
				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment1 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 15));
				personAssignment1.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));
				PersonAssignmentRepository.Add(personAssignment1);
				var personAssignment2 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 16));
				personAssignment2.AddActivity(activity, new DateTimePeriod(2015, 6, 16, 8, 2015, 6, 16, 17));
				PersonAssignmentRepository.Add(personAssignment2);

				uow.PersistAll();
			}

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			message.StartDateAsDateTime().Should().Be("2015-6-15 8:00".Utc());
			message.EndDateAsDateTime().Should().Be("2015-6-16 17:00".Utc());
		}


		[Test]
		public void ShouldSendAggregatedScheduleChangeMessageForMultipleDays()
		{
			using (var uow = Uow.Current().CreateAndOpenUnitOfWork())
			{
				var scenario = ScenarioFactory.CreateScenario(".", true, false);
				ScenarioRepository.Add(scenario);

				var person = PersonFactory.CreatePerson();
				PersonRepository.Add(person);

				var activity = new Activity(".");
				ActivityRepository.Add(activity);

				var personAssignment1 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 20));
				personAssignment1.AddActivity(activity, new DateTimePeriod(2015, 6, 20, 8, 2015, 6, 20, 17));
				PersonAssignmentRepository.Add(personAssignment1);
				var personAssignment2 = new PersonAssignment(person, scenario, new DateOnly(2015, 6, 15));
				personAssignment2.AddActivity(activity, new DateTimePeriod(2015, 6, 15, 8, 2015, 6, 15, 17));
				PersonAssignmentRepository.Add(personAssignment2);

				uow.PersistAll();
			}

			var message = MessageSender.NotificationsOfDomainType<IAggregatedScheduleChange>().Single();
			message.StartDateAsDateTime().Should().Be("2015-6-15 8:00".Utc());
			message.EndDateAsDateTime().Should().Be("2015-6-20 17:00".Utc());
		}
	}
}