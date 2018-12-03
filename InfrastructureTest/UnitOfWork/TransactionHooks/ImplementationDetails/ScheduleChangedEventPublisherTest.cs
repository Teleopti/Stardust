using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture]
	public class ScheduleChangedEventPublisherTest
	{
		[Test]
		public void Execute_WhenTheSchedulesHasChanged_ShouldSetTheStartDateTimeForTheMessageToTheStartTimeOfTheChangedSchedule()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherProbe();
			var target = new ScheduleChangedEventPublisher(publisher);
			target.AfterCompletion(new[] { rootChangeInfo });

			Assert.That(publisher.PublishedEvent.StartDateTime, Is.EqualTo(personAssignment.Period.StartDateTime));
			Assert.That(publisher.PublishedEvent.EndDateTime, Is.EqualTo(personAssignment.Period.EndDateTime));
		}

		[Test]
		public void ShouldSetTheStartDateTimeForTheMessageToTheStartTimeOfTheChangedScheduleMinus12HoursIfAbsence()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var period = new DateTimePeriod(new DateTime(2018, 5, 25, 2, 0, 0, DateTimeKind.Utc),
				new DateTime(2018, 5, 25, 6, 0, 0, DateTimeKind.Utc));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAbsence, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherProbe();
			var target = new ScheduleChangedEventPublisher(publisher);
			target.AfterCompletion(new[] {rootChangeInfo});

			Assert.That(publisher.PublishedEvent.StartDateTime, Is.EqualTo(period.StartDateTime.AddHours(-12)));
			Assert.That(publisher.PublishedEvent.EndDateTime, Is.EqualTo(period.EndDateTime));
		}

		[TestCase(-1, 2, -13, 24, Description = "PersonAbsence started before PersonAssignment but ended within it")]
		[TestCase(-1, 25, -13, 25, Description = "PersonAbsence contains PersonAssignment")]
		[TestCase(22, 25, 0, 25, Description = "PersonAbsence started within PersonAssignment after 12:00 but ended after it")]
		[TestCase(10, 25, -2, 25, Description = "PersonAbsence started within PersonAssignment before 12:00 but ended after it")]
		[TestCase(22, 23, 0, 24, Description = "PersonAbsence be contained within PersonAssignment")]
		public void ShouldSetThePeriodWithBothPersonAssignmentAndPersonAbsenceChanged(int personAbsencePeriodStart, int personAbsencePeriodEnd,
			int expectedPeriodStart, int expectedPeriodEnd)
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);

			var baseTimePoint = personAssignment.Period.StartDateTime;
			var period = new DateTimePeriod(baseTimePoint.AddHours(personAbsencePeriodStart), baseTimePoint.AddHours(personAbsencePeriodEnd));
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period);

			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAbsence, DomainUpdateType.Update);
			IRootChangeInfo rootChangeInfo1 = new RootChangeInfo(personAssignment, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherProbe();
			var target = new ScheduleChangedEventPublisher(publisher);
			target.AfterCompletion(new[] {rootChangeInfo, rootChangeInfo1});

			Assert.That(publisher.PublishedEvent.StartDateTime, Is.EqualTo(baseTimePoint.AddHours(expectedPeriodStart)));
			Assert.That(publisher.PublishedEvent.EndDateTime, Is.EqualTo(baseTimePoint.AddHours(expectedPeriodEnd)));
		}

		[Test]
		public void ShouldRetryPublishEventOneMoreTimeOnSqlException()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherTrowingSqlExeption();
			var target = new ScheduleChangedEventPublisher(publisher);
			try
			{
				target.AfterCompletion(new[] { rootChangeInfo });
			}
			catch (Exception)
			{
				//do nothing
			}
			Assert.AreEqual(2, publisher.Calls);
		}

		private class eventPopulatingPublisherProbe : IEventPopulatingPublisher
		{
			public ScheduleChangedEvent PublishedEvent;

			public void Publish(params IEvent[] events)
			{
				PublishedEvent = (ScheduleChangedEvent) events.Single();
			}
		}

		private class eventPopulatingPublisherTrowingSqlExeption : IEventPopulatingPublisher
		{
			public int Calls;

			public void Publish(params IEvent[] events)
			{
				Calls++;
				var sqlException = SqlExceptionConstructor.CreateSqlException("Timeout", 123);
				throw sqlException;
			}
		}
	}
}