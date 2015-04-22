using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.DomainTest.Aop.TestDoubles;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Aop
{
	[IoCTest]
	public class InfoLogAttributeTest : IRegisterInContainer
	{
		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<Service>().AsSelf().SingleInstance().ApplyAspects();
			var fakeLogger = new FakeLogger();
			builder.RegisterInstance(fakeLogger).AsSelf();
			builder.RegisterInstance(new FakeLogManagerWrapper(fakeLogger)).As<ILogManagerWrapper>();
		}

		public FakeLogger Logger;
		public Service Service;


		[Test]
		public void ShouldLogSomething()
		{
			Service.DoSomething(null);

			Logger.InfoMessage.Should().Not.Be.Null();
		}
	}

	public class Service
	{
		[InfoLog]
		public virtual void DoSomething(string s)
		{
			
		}
	}
}