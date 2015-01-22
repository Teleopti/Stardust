using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;

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

		private IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();
			var config = new IocConfiguration(new IocArgs { DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForTest() }, null);
			builder.RegisterModule(new CommonModule(config));
			builder.RegisterModule(new SyncEventsPublisherModule());
			builder.RegisterModule(new RtaModule(config));
			return builder.Build();
		}

	}
}
