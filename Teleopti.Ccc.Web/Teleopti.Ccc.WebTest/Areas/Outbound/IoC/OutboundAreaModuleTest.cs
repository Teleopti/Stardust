using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Outbound.core.IoC;
using Teleopti.Ccc.Web.Areas.Outbound.Controllers;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.IoC
{

    [TestFixture]
    class OutboundAreaModuleTest
    {
        private ContainerBuilder _containerBuilder;

        [SetUp]
        public void SetUp()
        {
            var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), null);
            _containerBuilder = new ContainerBuilder();           
            _containerBuilder.RegisterModule(new WebAppModule(configuration));
            _containerBuilder.RegisterModule(new OutboundScheduledResourcesProviderModule());
            _containerBuilder.RegisterModule(new OutboundAreaModule());

        }

        [Test]
        public void ShouldResolveOutboundRuleChecker()
        {
            using (var ioc = _containerBuilder.Build())
            {
                var checker = ioc.Resolve<IOutboundRuleChecker>();
                checker.Should().Not.Be.Null();
            }
        }

        [Test]
        public void ShouldResolveOutboundController()
        {
            using (var ioc = _containerBuilder.Build())
            {
                var controller = ioc.Resolve<OutboundController>();
                controller.Should().Not.Be.Null();
            }
        }

        [Test]
        public void ShouldResolveCampaignListProvider()
        {
            using (var ioc = _containerBuilder.Build())
            {
                var provider = ioc.Resolve<ICampaignListProvider>();
                provider.Should().Not.Be.Null();
            }
        }

        [Test]
        public void ShouldResolveCampaignSummaryViewModelFactory()
        {
            using (var ioc = _containerBuilder.Build())
            {
                var factory = ioc.Resolve<ICampaignSummaryViewModelFactory>();
                factory.Should().Not.Be.Null();
            }
        }

    }
}
