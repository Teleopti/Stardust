using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server
{
	public class RtaModuleTest
	{
		[Test]
		public void ShouldResolveTeleoptiRtaService()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<TeleoptiRtaService>()
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
		public void ShouldResolveAdherencePercentageReadModelUpdater()
		{
			using (var container = BuildContainer())
			{
				container.Resolve<IEnumerable<IHandleEvent<PersonOutOfAdherenceEvent>>>()
					.Select(x => x.GetType())
					.Should().Contain(typeof (AdherencePercentageReadModelUpdater));
			}
		}

		[Test]
		public void ShouldCachePersonOrganizationProvider()
		{
			using (var container = BuildContainer())
			{
				var builder = new ContainerBuilder();
				var reader = MockRepository.GenerateMock<IPersonOrganizationReader>();
				reader.Stub(x => x.PersonOrganizationData()).Return(new PersonOrganizationData[] { });
				builder.RegisterInstance(reader).As<IPersonOrganizationReader>();
				builder.Update(container);

				var orgReader1 = container.Resolve<IPersonOrganizationProvider>();
				var orgReader2 = container.Resolve<IPersonOrganizationProvider>();
				orgReader1.PersonOrganizationData().Should().Be.SameInstanceAs(orgReader2.PersonOrganizationData());
			}
		}

		[Test, Ignore]
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
			builder.RegisterModule(new InitializeModule(DataSourceConfigurationSetter.ForTest()));
			builder.RegisterModule(new RtaModule(config));
			return builder.Build();
		}

		private static IContainer BuildContainerWithToggle(Toggles toggle, bool value)
		{
			var builder = new ContainerBuilder();
			var config = new IocConfiguration(new IocArgs(), ToggleManager(toggle, value));
			builder.RegisterModule(new CommonModule(config));
			builder.RegisterModule(new LocalInMemoryEventsPublisherModule());
			builder.RegisterModule(new RtaModule(config));
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
