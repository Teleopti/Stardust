using System;
using System.Configuration;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Interfaces;
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
			var builder = Builder();
			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			builder.Register(c => toggleManager).As<IToggleManager>();
			return builder.Build();
		}

		public class GodModule : Module
		{
			protected override void Load(ContainerBuilder builder)
			{
				builder.RegisterModule<DateAndTimeModule>();
				builder.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>().SingleInstance();
				builder.RegisterType<NewtonsoftJsonDeserializer>().As<IJsonDeserializer>().SingleInstance();
				builder.RegisterModule(new ToggleNetModule(ConfigurationManager.AppSettings["FeatureToggle"], ConfigurationManager.AppSettings["ToggleMode"]));
				builder.RegisterModule<MessageBrokerModule>();
			}
		}

	}
}