using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	public class OutboundAreaModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new WebAppModule(configuration));
			_containerBuilder.RegisterModule(new OutboundAreaModule());
		}

		[Test]
		public void ShouldResolveOutboundSkillCreator()
		{
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IOutboundSkillCreator>()
								 .Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveOutboundSkillPersister()
		{
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IOutboundSkillPersister>()
								 .Should().Not.Be.Null();
			}
		}

		//IOutboundCampaignPersister
		[Test]
		public void ShouldResolveOutboundCampaignPersister()
		{
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<IOutboundCampaignPersister>()
								 .Should().Not.Be.Null();
			}
		}
	}
}