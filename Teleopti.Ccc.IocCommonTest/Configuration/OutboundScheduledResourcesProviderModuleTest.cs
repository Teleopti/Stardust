using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class OutboundScheduledResourcesProviderModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			_containerBuilder = new ContainerBuilder();
			var configuration = CommonModule.ForTest();
			_containerBuilder.RegisterModule(configuration);
			_containerBuilder.RegisterModule(new OutboundScheduledResourcesProviderModule());
		}

		[Test]
		public void ShouldResolveOutboundScheduledResourcesProvider()
		{
			using (var ioc = _containerBuilder.Build())
			{
				var provider = ioc.Resolve<IOutboundScheduledResourcesProvider>();
				provider.Should().Not.Be.Null();
			}
		}
	}
}