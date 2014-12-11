using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[TestFixture]
	public class SendMessageTest
	{
		[Test]
		public void ShouldSendMessageWhenStateChangeEventIsHandledSuccessfullyForNewModel()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var transactionSync = new ControllableLiteTransactionSyncronization();
			var messageBroker = MockRepository.GenerateMock<IMessageSender>();
			var notificationCreator = MockRepository.GenerateMock<INotificationCreator>();
			var target = new AdherenceDetailsReadModelUpdater(persister, transactionSync, messageBroker, notificationCreator);
			var @event = new PersonStateChangedEvent
			{
				Datasource = "datasource",
				BusinessUnitId = Guid.NewGuid()
			};
			var notification = new Interfaces.MessageBroker.Notification
			{
				DataSource = @event.Datasource,
				BusinessUnitId = @event.BusinessUnitId.ToString(),
				DomainType = "AdherenceDetailsReadModelUpdatedMessage"
			};
			notificationCreator.Stub(x => x.Create(@event.Datasource, @event.BusinessUnitId, "AdherenceDetailsReadModelUpdatedMessage")).Return(notification);
			target.Handle(@event);
			transactionSync.RunNow();

			messageBroker.AssertWasCalled(x => x.Send(notification));
		}

		[Test]
		public void ShouldSendMessageWhenStateChangeEventIsHandledSuccessfullyForOldModel()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var transactionSync = new ControllableLiteTransactionSyncronization();
			var messageBroker = MockRepository.GenerateMock<IMessageSender>();
			var notificationCreator = MockRepository.GenerateMock<INotificationCreator>();
			var target = new AdherenceDetailsReadModelUpdater(persister, transactionSync, messageBroker, notificationCreator);
			var personId = Guid.NewGuid();
			var @event = new PersonStateChangedEvent
			{
				Datasource = "datasource",
				BusinessUnitId = Guid.NewGuid()
			};
			var notification = new Interfaces.MessageBroker.Notification
			{
				DataSource = @event.Datasource,
				BusinessUnitId = @event.BusinessUnitId.ToString(),
				DomainType = "AdherenceDetailsReadModelUpdatedMessage"
			};
			notificationCreator.Stub(x => x.Create(@event.Datasource, @event.BusinessUnitId, "AdherenceDetailsReadModelUpdatedMessage")).Return(notification);
			target.Handle(new PersonActivityStartEvent { PersonId = personId, StartTime = "2014-11-17 8:00".Utc(), Name = "Phone", InAdherence = true });
			target.Handle(@event);
			transactionSync.RunNow();

			messageBroker.AssertWasCalled(x => x.Send(notification));
		}
	}
}
