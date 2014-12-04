using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class ServiceBusEventPublisherTest
	{
		[Test]
		public void ShouldSendToBus()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var target = new EventPopulatingPublisher(new ServiceBusEventPublisher(serviceBusSender), new DummyContextPopulator());
			var @event = new Event();

			target.Publish(@event);

			serviceBusSender.AssertWasCalled(x => x.Send(@event, true));
		}

	}
}