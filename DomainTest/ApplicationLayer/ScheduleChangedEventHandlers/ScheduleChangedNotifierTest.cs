using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ScheduleChangedNotifierTest
	{
		[Test]
		public void ShouldSendBrokerMessageOnScheduleChange()
		{
			var broker = MockRepository.GenerateMock<IMessageCreator>();
			var handler = new ScheduleChangedNotifier(broker);

			var message = new ScheduleChangedEvent
				{
					InitiatorId = Guid.NewGuid(),
					LogOnBusinessUnitId = Guid.NewGuid(),
					LogOnDatasource = "My Data",
					StartDateTime = new DateTime(2010,1,1),
					EndDateTime = new DateTime(2010,1,31),
					PersonId = Guid.NewGuid(),
					ScenarioId = Guid.NewGuid()
				};
			handler.Handle(message);

			broker.AssertWasCalled(x => x.Send(
				message.LogOnDatasource,
				message.LogOnBusinessUnitId,
				message.StartDateTime,
				message.EndDateTime,
				message.InitiatorId,
				message.ScenarioId,
				typeof (Scenario),
				message.PersonId,
				typeof (IScheduleChangedEvent),
				DomainUpdateType.NotApplicable,
				null));
		}

		[Test]
		public void ShouldNotSendBrokerMessageOnScheduleChangeOnInitialLoad()
		{
			var broker = MockRepository.GenerateMock<IMessageCreator>();
			var handler = new ScheduleChangedNotifier(broker);

			var message = new ScheduleChangedEvent
			{
				InitiatorId = Guid.NewGuid(),
				LogOnBusinessUnitId = Guid.NewGuid(),
				LogOnDatasource = "My Data",
				StartDateTime = new DateTime(2010, 1, 1),
				EndDateTime = new DateTime(2010, 1, 31),
				PersonId = Guid.NewGuid(),
				ScenarioId = Guid.NewGuid(),
				SkipDelete = true
			};
			handler.Handle(message);

			broker.AssertWasNotCalled(x => x.Send(
				message.LogOnDatasource,
				message.LogOnBusinessUnitId,
				message.StartDateTime,
				message.EndDateTime,
				message.InitiatorId,
				message.ScenarioId,
				typeof(Scenario),
				message.PersonId,
				typeof(IScheduleChangedEvent),
				DomainUpdateType.NotApplicable,
				null));
		}
	}
}
