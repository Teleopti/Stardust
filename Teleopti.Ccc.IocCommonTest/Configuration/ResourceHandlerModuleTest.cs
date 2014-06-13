using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class ResourceHandlerModuleTest
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<ResourceHandlerModule>();
		}

		[Test]
		public void UserTextTranslatorShouldBeRegisteredAsSingleton()
		{
			using (var container = containerBuilder.Build())
			{
				var obj1 = container.Resolve<IUserTextTranslator>();
				var obj2 = container.Resolve<IUserTextTranslator>();
				obj1.Should().Not.Be.Null();
				obj1.Should().Be.SameInstanceAs(obj2);
			}
		}
	}
}
