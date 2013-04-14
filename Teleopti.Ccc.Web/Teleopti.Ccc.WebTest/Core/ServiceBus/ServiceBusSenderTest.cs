using System;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.ServiceBus;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.WebTest.Core.ServiceBus
{
	[TestFixture]
	public class ServiceBusSenderTest
	{
		[Test]
		public void ShouldEnsureBus()
		{
			using (var sender = new ServiceBusSender())
			{
				sender.EnsureBus();
				sender.FieldValue<IContainer>("_customHost").Should().Not.Be.Null();
				sender.FieldValue<bool>("_isRunning").Should().Be.False(); //due to miss config file
			}
		}
		
		[Test]
		public void ShouldSend()
		{
			using (var sender = new TestServiceBusSender())
			{
				var bus = MockRepository.GenerateMock<IOnewayBus>();
				sender.Resolveable = bus;
				sender.EnsureBus();
				var message = new TestMessage();
				sender.Send(message);
				bus.AssertWasCalled(x => x.Send(message));
			}
		}

		public class TestMessage : RaptorDomainMessage
		{
			public override Guid Identity { get { return Guid.Empty; } }
		}

		public class TestServiceBusSender : ServiceBusSender
		{

			public object Resolveable { get; set; }

			protected override T Resolve<T>()
			{
				if (Resolveable is T)
					return (T)Resolveable;
				return default(T);
			}
		}
	}
}