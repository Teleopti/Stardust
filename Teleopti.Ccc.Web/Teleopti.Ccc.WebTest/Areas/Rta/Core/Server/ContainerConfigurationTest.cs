using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server
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

	}
}
