using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks.ImplementationDetails
{
	[TestFixture]
	public class ScheduleMessageSenderTest
	{
		[Test]
		public void Execute_WhenTheSchedulesHasChanged_ShouldSetTheStartDateTimeForTheMessageToTheStartTimeOfTheChangedSchedule()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherProbe();
			var target = new ScheduleChangedEventPublisher(publisher, new Now());
			target.AfterFlush(new[] { rootChangeInfo });

			Assert.That(publisher.PublishedEvent.StartDateTime, Is.EqualTo(personAssignment.Period.StartDateTime));
		}

		[Test]
		public void ShouldRetryPublishEventOneMoreTimeOnSqlException()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Update);
			var publisher = new eventPopulatingPublisherTrowingSqlExeption();
			var target = new ScheduleChangedEventPublisher(publisher, new Now());
			try
			{
				target.AfterFlush(new[] { rootChangeInfo });
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