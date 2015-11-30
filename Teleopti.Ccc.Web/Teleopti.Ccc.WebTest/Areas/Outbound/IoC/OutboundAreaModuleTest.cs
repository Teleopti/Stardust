using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.IoC;
using Teleopti.Ccc.Web.Areas.Outbound.Controllers;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.IoC
{
	[TestFixture]
	public class OutboundAreaModuleTest
	{
		private ContainerBuilder _containerBuilder;
		private string _requestTag;

		[SetUp]
		public void SetUp()
		{
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new WebAppModule(configuration));
			_containerBuilder.RegisterModule(new OutboundAreaModule());
			_requestTag = "AutofacWebRequest";

		}

		[Test]
		public void ScheduleResourcesProviderShouldBeDifferentForDifferentRequest()
		{
			using (var ioc = _containerBuilder.Build())
			{
				IOutboundScheduledResourcesProvider holder1, holder2;

				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					holder1 = scope.Resolve<IOutboundScheduledResourcesProvider>();					
				}

				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					holder2 = scope.Resolve<IOutboundScheduledResourcesProvider>();
				}

				holder1.Should().Not.Be.EqualTo(holder2);
			}
		}

		[Test]
		public void ShouldResolveOutboundController()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					var controller = scope.Resolve<OutboundController>();
					controller.Should().Not.Be.Null();
				}
			}
		}

		[Test]
		public void ShouldResolveCampaignListProvider()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					var provider = scope.Resolve<ICampaignListProvider>();
					provider.Should().Not.Be.Null();
				}

			}
		}
		
		[Test]
		public void ShouldResolveOutboundCampaignPersister()
		{
			using (var ioc = _containerBuilder.Build())
			{
				using (var scope = ioc.BeginLifetimeScope(_requestTag))
				{
					scope.Resolve<IOutboundCampaignPersister>()
						.Should().Not.Be.Null();
				}
			}
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
	}
}
