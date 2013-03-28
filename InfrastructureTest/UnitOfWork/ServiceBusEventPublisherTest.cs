using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class ServiceBusEventPublisherTest
	{
		[Test]
		public void ShouldSendToBus()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var target = new ServiceBusEventPublisher(serviceBusSender);
			var @event = new Event();
			serviceBusSender.Stub(x => x.EnsureBus()).Return(true);

			target.Publish(@event);

			serviceBusSender.AssertWasCalled(x => x.Send(@event));
		}

		[Test]
		public void ShouldThrowIfBusCanNotBeEnsured()
		{
			var serviceBusSender = MockRepository.GenerateMock<IServiceBusSender>();
			var target = new ServiceBusEventPublisher(serviceBusSender);
			var @event = new Event();
			serviceBusSender.Stub(x => x.EnsureBus()).Return(false);

			Assert.Throws<Exception>(() => target.Publish(@event));
		}
	}
}