using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.InfrastructureTest.Aop
{
	[TestFixture]
	public class Bug32045
	{
		[Test]
		public void ShouldThrowOnMissingDependency()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterType<MyClass>().As<IInterface>().ApplyAspects();
			var container = builder.Build();

			Assert.Throws<DependencyResolutionException>(() => container.Resolve<IInterface>());
		}

		public interface IInterface
		{
		}

		public class MyClass : IInterface
		{
			public MyClass(UnregisteredDependency dependency)
			{
			}
		}

		public class UnregisteredDependency
		{
		}

	}
}