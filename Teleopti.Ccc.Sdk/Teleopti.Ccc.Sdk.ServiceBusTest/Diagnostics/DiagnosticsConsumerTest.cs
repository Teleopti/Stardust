﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.Diagnostics;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Diagnostics
{
	public class DiagnosticsConsumerTest
	{
		[Test]
		public void ShouldSendNotificationWithProcessInformation()
		{
			var broker = MockRepository.GenerateMock<IMessageBroker>();
			var target = new DiagnosticsConsumer(broker);

			var businessUnitId = Guid.NewGuid();
			var datasourceName = "datasource";
			var timestamp = DateTime.UtcNow;
			var initiatorId = Guid.NewGuid();
			target.Consume(new DiagnosticsMessage{BusinessUnitId = businessUnitId,Datasource = datasourceName,Timestamp = timestamp,InitiatorId = initiatorId});

			broker.AssertWasCalled(
				x =>
					x.SendEventMessage("", Guid.Empty, DateOnly.Today, DateOnly.Today, Guid.Empty, Guid.Empty,
						typeof (TeleoptiDiagnosticsInformation), DomainUpdateType.NotApplicable, new byte[] {}),
				o =>
					o.Constraints(Rhino.Mocks.Constraints.Is.Equal(datasourceName), Rhino.Mocks.Constraints.Is.Equal(businessUnitId),
						Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything(),
						Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Equal(initiatorId),
						Rhino.Mocks.Constraints.Is.Equal(typeof (TeleoptiDiagnosticsInformation)), Rhino.Mocks.Constraints.Is.Anything(),
						Rhino.Mocks.Constraints.Is.Matching(new Predicate<byte[]>(bytes => bytes.Length > 0))));
		}
	}
}
