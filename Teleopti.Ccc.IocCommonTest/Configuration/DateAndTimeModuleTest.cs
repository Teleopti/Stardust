using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class DateAndTimeModuleTest
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule(new DateAndTimeModule());
		}


		[Test]
		public void NowShouldBeRegisteredAsSingleton()
		{
			using(var container = containerBuilder.Build())
			{
				var obj1 = container.Resolve<INow>();
				var obj2 = container.Resolve<INow>();
				obj1.Should().Not.Be.Null();
				obj1.Should().Be.SameInstanceAs(obj2);
			}
		}
	}
}