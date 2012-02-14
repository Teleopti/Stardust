using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class InitializeModuleTest 
	{
		private ContainerBuilder containerBuilder;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterModule<InitializeModule>();
		}
		
		[Test]
		public void InitializeApplicationIsWired()
		{
			using (var container = containerBuilder.Build())
			{
				var init = container.Resolve<IInitializeApplication>();
                Assert.IsNotNull(init);
				//init.Should().Not.Be.Null();
			}
		}

		[Test]
		public void InitializeApplicationShouldBeSingleton()
		{
			using (var container = containerBuilder.Build())
			{
				var init = container.Resolve<IInitializeApplication>();
                Assert.AreSame(container.Resolve<IInitializeApplication>(), init);
				//init.Should().Be.SameInstanceAs(container.Resolve<IInitializeApplication>());
			}
		}
	}
}