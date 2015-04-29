using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.MultipleConfig;
using Teleopti.Ccc.Web.Areas.Outbound.core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	public class OutboundAreaModuleTest
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void Setup()
		{
			var configuration = new IocConfiguration(new IocArgs(new AppConfigReader()), null);
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(new CommonModule(configuration));
			_containerBuilder.RegisterModule(new OutboundAreaModule());
		}

		[Test]
		public void ShouldResolveOutboundSkillCreator()
		{
			using (var container = _containerBuilder.Build())
			{
				container.Resolve<OutboundSkillCreator>()
								 .Should().Not.Be.Null();
			}
		} 
	}
}