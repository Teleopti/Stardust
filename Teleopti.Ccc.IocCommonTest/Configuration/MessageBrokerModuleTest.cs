using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class MessageBrokerModuleTest
	{
		[Test]
		public void ShouldResolveMessageSender()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<IMessageSender>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveMessageBrokerCompositeClient()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<IMessageBrokerComposite>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveHttpSender()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<IMessageSender>()
					.Should().Be.OfType<HttpSender>();
			}
		}

		[Test]
		public void ShouldResolveKeepAliveStrategies()
		{
			var config = new IocConfiguration(new IocArgs(new ConfigReader()) { MessageBrokerListeningEnabled = true }, null);
			using (var container = BuildContainer(config))
			{
				container.Resolve<IEnumerable<IConnectionKeepAliveStrategy>>()
					.Select(x => x.GetType())
					.Should().Have.SameValuesAs(new[] { typeof(RecreateOnNoPingReply), typeof(RestartOnClosed) });
			}
		}

		[Test]
		public void ShouldNotUseSignalRIfListeningDisabled()
		{
			var config = new IocConfiguration(
				new IocArgs(new ConfigReader()) { MessageBrokerListeningEnabled = false }, null
				);
			using (var container = BuildContainer(config))
			{
				container.Resolve<ISignalRClient>().Should().Be.OfType<DisabledSignalRClient>();
				container.Resolve<IMessageSender>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveSignalRClientFromSharedContainer()
		{
			var signalRClient = MockRepository.GenerateMock<ISignalRClient>();
			var builder = new ContainerBuilder();
			builder.RegisterInstance(signalRClient).As<ISignalRClient>();
			var sharedContainer = builder.Build();
			using (var container = BuildContainer(new IocConfiguration(new IocArgs(new ConfigReader()){SharedContainer = sharedContainer}, null)))
			{
				container.Resolve<ISignalRClient>().Should().Be.SameInstanceAs(signalRClient);
			}
		}

		private IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()), null)));
			return builder.Build();
		}

		private IContainer BuildContainer(IIocConfiguration configuration)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(configuration));
			return builder.Build();
		}

		private static IContainer BuildContainerWithToggle(Toggles toggle, bool value)
		{
			var builder = new ContainerBuilder();
			var config = new IocConfiguration(new IocArgs(new ConfigReader()), ToggleManager(toggle, value));
			builder.RegisterModule(new CommonModule(config));
			return builder.Build();
		}

		private static IToggleManager ToggleManager(Toggles toggle, bool value)
		{
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			return toggleManager;
		}
	}
}