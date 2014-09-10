using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
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
			using (var container = BuildContainer())
			{
				container.Resolve<IEnumerable<IConnectionKeepAliveStrategy>>()
					.Select(x => x.GetType())
					.Should().Have.SameValuesAs(new[] { typeof(RecreateOnNoPingReply), typeof(RestartOnClosed) });
			}
		}

		[Test]
		public void ShouldNotUseSignalRIfListeningDisabledAndHttpSenderEnabled()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new GodModule {MessageBrokerListeningEnabled = false});
			using (var container = BuildContainerWithToggle(builder, Toggles.Messaging_HttpSender_29205, true))
			{
				container.Resolve<ISignalRClient>().Should().Be.OfType<DisabledSignalRClient>();
				container.Resolve<IMessageSender>().Should().Not.Be.Null();
			}
		}

		private IContainer BuildContainer()
		{
			return Builder().Build();
		}

		private static ContainerBuilder Builder()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<GodModule>();
			return builder;
		}

		private static IContainer BuildContainerWithToggle(Toggles toggle, bool value)
		{
			return BuildContainerWithToggle(Builder(), toggle, value);
		}

		private static IContainer BuildContainerWithToggle(ContainerBuilder builder, Toggles toggle, bool value)
		{
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			builder.Register(c => toggleManager).As<IToggleManager>();
			return builder.Build();
		}
	}
}