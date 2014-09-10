using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.Rta.ServerTest
{
	public class ContainerConfigurationTest
	{
		[Test]
		public void ShouldResolveRtaDataHandler()
		{
			using (var container = new ContainerConfiguration().Configure().Build())
			{
				container.Resolve<IRtaDataHandler>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveAdherenceAggregator()
		{
			using (var container = new ContainerConfiguration().Configure().Build())
			{
				container.Resolve<IEnumerable<IActualAgentStateHasBeenSent>>()
					.Single().GetType().Should().Be<AdherenceAggregator>();
			}
		}

		[Test]
		public void ShouldResolveAdherenceAggregatorInitializor()
		{
			using (var container = new ContainerConfiguration().Configure().Build())
			{
				container.Resolve<AdherenceAggregatorInitializor>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldCachePersonOrganizationProvider()
		{
			var builder = new ContainerConfiguration().Configure();
			var reader = MockRepository.GenerateMock<IPersonOrganizationReader>();
			reader.Stub(x => x.LoadAll()).Return(new PersonOrganizationData[] {});
			builder.RegisterInstance(reader).As<IPersonOrganizationReader>();
			using (var container = builder.Build())
			{
				var orgReader1 = container.Resolve<IPersonOrganizationProvider>();
				var orgReader2 = container.Resolve<IPersonOrganizationProvider>();
				orgReader1.LoadAll().Should().Be.SameInstanceAs(orgReader2.LoadAll());
			}
		}

		[Test]
		public void ShouldResolveMessageSender()
		{
			using (var container = new ContainerConfiguration().Configure().Build())
			{
				container.Resolve<IMessageSender>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveSignalRSender()
		{
			using (var container = containerWithToggle(Toggles.Messaging_HttpSender_29205, false))
			{
				container.Resolve<IMessageSender>()
					.Should().Be.OfType<SignalRSender>();
			}
		}

		[Test]
		public void ShouldResolveHttpSender()
		{
			using (var container = containerWithToggle(Toggles.Messaging_HttpSender_29205, true))
			{
				container.Resolve<IMessageSender>()
					.Should().Be.OfType<HttpSender>();
			}
		}

		//[Test]
		//public void ShouldResolveKeepAliveStrategies()
		//{
		//	using (var container = new ContainerConfiguration().Configure().Build())
		//	{
		//		container.Resolve<IEnumerable<IConnectionKeepAliveStrategy>>()
		//			.Select(x => x.GetType())
		//			.Should().Have.SameValuesAs(new[] {typeof (RecreateOnNoPingReply), typeof (RestartOnClosed)});
		//	}
		//}

		private static IContainer containerWithToggle(Toggles toggle, bool value)
		{
			var builder = new ContainerConfiguration().Configure();

			var toggleManager = MockRepository.GenerateStub<IToggleManager>();
			toggleManager.Stub(x => x.IsEnabled(toggle)).Return(value);
			builder.Register(c => toggleManager).As<IToggleManager>();

			return builder.Build();
		}


	}
}
