using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class DateAndTimeModuleTest
	{
		private ContainerBuilder builder;

		[SetUp]
		public void Setup()
		{
			builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			builder.Register(c => MockRepository.GenerateMock<IUserTimeZone>()).As<IUserTimeZone>(); //no impl of IUserTimeZone available outside web yet
		}


		[Test]
		public void NowShouldBeRegisteredAsSingleton()
		{
			using(var container = builder.Build())
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
			using (var container = builder.Build())
			{
				var obj1 = container.Resolve<ITime>();
				var obj2 = container.Resolve<ITime>();
				obj1.Should().Not.Be.Null();
				obj1.Should().Be.SameInstanceAs(obj2);
			}
		}
	}
}