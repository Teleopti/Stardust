using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Container
{
	public class MessageBrokerTest
	{
		[Test]
		public void ShouldResolveMessageSender()
		{
			
			using (var container = new ContainerBuilder().Build())
			{
				new ContainerConfiguration(container).Configure();
				container.Resolve<IMessageSender>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveSignalRSender()
		{
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(Toggles.Messaging_HttpSender_29205)).Return(false);
			using (var container = new ContainerBuilder().Build())
			{
				new ContainerConfiguration(container).Configure();



				var temp = new ContainerBuilder();
				temp.Register(c => toggleManager).As<IToggleManager>();
				temp.Update(container);


				container.Resolve<IMessageSender>()
					.Should().Be.OfType<SignalRSender>();
			}
		}


		[Test]
		public void ShouldResolveHttpSender()
		{
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(Toggles.Messaging_HttpSender_29205)).Return(true);
			using (var container = new ContainerBuilder().Build())
			{
				new ContainerConfiguration(container).Configure();
				var temp = new ContainerBuilder();
				temp.Register(c => toggleManager).As<IToggleManager>();
				temp.Update(container);
				container.Resolve<IMessageSender>()
					.Should().Be.SameInstanceAs(container.Resolve<HttpSender>());
			}
		}

	}
}