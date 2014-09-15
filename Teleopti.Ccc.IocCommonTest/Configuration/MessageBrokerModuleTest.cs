using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
		public void ShouldResolveSignalRSender()
		{
			using (var container = BuildContainerWithToggle(Toggles.Messaging_HttpSender_29205, false))
			{
				container.Resolve<IMessageSender>()
					.Should().Be.SameInstanceAs(container.Resolve<SignalRSender>());
			}
		}

		[Test]
		public void ShouldResolveHttpSender()
		{
			using (var container = BuildContainerWithToggle(Toggles.Messaging_HttpSender_29205, true))
			{
				container.Resolve<IMessageSender>()
					.Should().Be.SameInstanceAs(container.Resolve<HttpSender>());
			}
		}

		[Test]
		public void ShouldResolveKeepAliveStrategies()
		{
			var config = new IocConfiguration(new IocArgs {MessageBrokerListeningEnabled = true}, null);
			using (var container = BuildContainer(config))
			{
				container.Resolve<IEnumerable<IConnectionKeepAliveStrategy>>()
					.Select(x => x.GetType())
					.Should().Have.SameValuesAs(new[] { typeof(RecreateOnNoPingReply), typeof(RestartOnClosed) });
			}
		}

		[Test]
		public void ShouldNotUseSignalRIfListeningDisabledAndHttpSenderEnabled()
		{
			var config = new IocConfiguration(
				new IocArgs {MessageBrokerListeningEnabled = false},
				ToggleManager(Toggles.Messaging_HttpSender_29205, true)
				);
			using (var container = BuildContainer(config))
			{
				container.Resolve<ISignalRClient>().Should().Be.OfType<DisabledSignalRClient>();
				container.Resolve<IMessageSender>().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldStillUseSignalRIfListeningDisabledAndHttpSenderDisabled()
		{
			var config = new IocConfiguration(
				new IocArgs { MessageBrokerListeningEnabled = false },
				ToggleManager(Toggles.Messaging_HttpSender_29205, false)
				);
			using (var container = BuildContainer(config))
			{
				container.Resolve<ISignalRClient>().Should().Be.OfType<SignalRClient>();
			}
		}

		private IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(), null)));
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
			var config = new IocConfiguration(new IocArgs(), ToggleManager(toggle, value));
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