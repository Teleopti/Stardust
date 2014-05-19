using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
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
			containerBuilder.Register(c => MockRepository.GenerateMock<IUserTimeZone>()).As<IUserTimeZone>(); //no impl of IUserTimeZone available outside web yet
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


		[Test]
		public void TimeShouldBeRegisteredAsSingleton()
		{
			using (var container = containerBuilder.Build())
			{
				var obj1 = container.Resolve<ITime>();
				var obj2 = container.Resolve<ITime>();
				obj1.Should().Not.Be.Null();
				obj1.Should().Be.SameInstanceAs(obj2);
			}
		}
	}
}