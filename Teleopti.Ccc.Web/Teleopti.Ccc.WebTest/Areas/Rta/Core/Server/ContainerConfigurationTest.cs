using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server
{
	public class ContainerConfigurationTest
	{
		[Test]
		public void ShouldResolveRtaDataHandler()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<IRtaDataHandler>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveAdherenceAggregator()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<IEnumerable<IActualAgentStateHasBeenSent>>()
					.Single().GetType().Should().Be<AdherenceAggregator>();
			}
		}

		[Test]
		public void ShouldResolveAdherenceAggregatorInitializor()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<AdherenceAggregatorInitializor>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldCachePersonOrganizationProvider()
		{
			var builder = new ContainerBuilder();
			var config = new IocConfiguration(new IocArgs(), null);
			builder.RegisterModule(new CommonModule(config));
			builder.RegisterModule(new UnitOfWorkModule());
			builder.RegisterModule(new AuthenticationModule());
			builder.RegisterModule(new LocalServiceBusEventsPublisherModule());
			var mbCacheModule = new MbCacheModule(null);
			builder.RegisterModule(mbCacheModule);
			builder.RegisterModule(new RtaCommonModule(mbCacheModule, config));

			var reader = MockRepository.GenerateMock<IPersonOrganizationReader>();
			reader.Stub(x => x.LoadAll()).Return(new PersonOrganizationData[] { });
			builder.RegisterInstance(reader).As<IPersonOrganizationReader>();

			using (var container = builder.Build())
			{
				var orgReader1 = container.Resolve<IPersonOrganizationProvider>();
				var orgReader2 = container.Resolve<IPersonOrganizationProvider>();
				orgReader1.LoadAll().Should().Be.SameInstanceAs(orgReader2.LoadAll());
			}
		}

		[Test]
		public void ShouldResolveAgentStateChangedCommandHandler()
		{
			using (var container = BuildContainerWithToggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783, true))
			{
				container.Resolve<IEnumerable<IActualAgentStateHasBeenSent>>()
					.Select(o => o.GetType())
					.Should().Contain(typeof (AgentStateChangedCommandHandler));
			}
		}

		[Test]
		public void ShouldNotResolveAgentStateChangedCommandHandler()
		{
			using (var container = BuildContainerWithToggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783, false))
			{
				container.Resolve<IEnumerable<IActualAgentStateHasBeenSent>>()
					.Select(o => o.GetType())
					.Should().Not.Contain(typeof(AgentStateChangedCommandHandler));
			}
		}

		private IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();
			var config = new IocConfiguration(new IocArgs(), null);
			builder.RegisterModule(new CommonModule(config));
			builder.RegisterModule(new UnitOfWorkModule());
			builder.RegisterModule(new AuthenticationModule());
			builder.RegisterModule(new LocalServiceBusEventsPublisherModule());
			var mbCacheModule = new MbCacheModule(null);
			builder.RegisterModule(mbCacheModule);
			builder.RegisterModule(new RtaCommonModule(mbCacheModule, config));
			return builder.Build();
		}

		private static IContainer BuildContainerWithToggle(Toggles toggle, bool value)
		{
			var builder = new ContainerBuilder();
			var config = new IocConfiguration(new IocArgs(), ToggleManager(toggle, value));
			builder.RegisterModule(new CommonModule(config));
			builder.RegisterModule(new UnitOfWorkModule());
			builder.RegisterModule(new AuthenticationModule());
			builder.RegisterModule(new LocalServiceBusEventsPublisherModule());
			var mbCacheModule = new MbCacheModule(null);
			builder.RegisterModule(mbCacheModule);
			builder.RegisterModule(new RtaCommonModule(mbCacheModule, config));
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
