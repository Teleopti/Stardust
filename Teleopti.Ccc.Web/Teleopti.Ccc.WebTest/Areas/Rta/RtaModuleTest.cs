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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
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
					.Where(x => x is AdherencePercentageReadModelUpdater)
					.Should().Have.Count.EqualTo(1);
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

				var resolved = container.Resolve<IPersonOrganizationProvider>();
				resolved.PersonOrganizationData().Should().Be.SameInstanceAs(resolved.PersonOrganizationData());
			}
		}

		[Test]
		public void ShouldResolvePercentageFeatureEventHandlers()
		{
			using (var container = BuildContainerWithToggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783, true))
			{
				container.Resolve<IShiftEventPublisher>().Should().Be.OfType<ShiftEventPublisher>();
				container.Resolve<IAdherenceEventPublisher>().Should().Be.OfType<AdherenceEventPublisher>();
				container.Resolve<IStateEventPublisher>().Should().Be.OfType<StateEventPublisher>();
				container.Resolve<IActivityEventPublisher>().Should().Be.OfType<ActivityEventPublisher>();
			}
		}

		[Test]
		public void ShouldNotResolvePercentageFeatureEventHandlers()
		{
			using (var container = BuildContainerWithToggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783, false))
			{
				container.Resolve<IShiftEventPublisher>().Should().Be.OfType<NoEvents>();
				container.Resolve<IAdherenceEventPublisher>().Should().Be.OfType<NoEvents>();
			}
		}

		[Test]
		public void ShouldResolveAdherenceDetailsFeatureEventHandlers()
		{
			using (var container = BuildContainerWithToggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285, true))
			{
				container.Resolve<IShiftEventPublisher>().Should().Be.OfType<ShiftEventPublisher>();
				container.Resolve<IAdherenceEventPublisher>().Should().Be.OfType<AdherenceEventPublisher>();
				container.Resolve<IStateEventPublisher>().Should().Be.OfType<StateEventPublisher>();
				container.Resolve<IActivityEventPublisher>().Should().Be.OfType<ActivityEventPublisher>();
			}
		}

		[Test]
		public void ShouldNotResolveAdherenceDetailsFeatureEventHandlers()
		{
			using (var container = BuildContainerWithToggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285, false))
			{
				container.Resolve<IShiftEventPublisher>().Should().Be.OfType<NoEvents>();
				container.Resolve<IAdherenceEventPublisher>().Should().Be.OfType<NoEvents>();
				container.Resolve<IStateEventPublisher>().Should().Be.OfType<NoEvents>();
				container.Resolve<IActivityEventPublisher>().Should().Be.OfType<NoEvents>();
			}
		}

		private IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();
			var config = new IocConfiguration(new IocArgs { DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForTest() }, null);
			builder.RegisterModule(new CommonModule(config));
			builder.RegisterModule(new SyncEventsPublisherModule());
			builder.RegisterModule(new RtaModule(config));
			return builder.Build();
		}

		private static IContainer BuildContainerWithToggle(Toggles toggle, bool value)
		{
			var builder = new ContainerBuilder();
			var applicationData = MockRepository.GenerateMock<IApplicationData>();
			var config = new IocConfiguration(new IocArgs { DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForTest() }, ToggleManager(toggle, value));
			builder.RegisterModule(new CommonModule(config));
			builder.RegisterModule(new SyncEventsPublisherModule());
			builder.RegisterModule(new RtaModule(config));
			builder.RegisterInstance(applicationData);
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
