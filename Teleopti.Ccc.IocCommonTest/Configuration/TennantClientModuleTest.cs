using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class TennantClientModuleTest
	{
		private ContainerBuilder builder;

		[Test]
		public void CanResolveTennantQuerier()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IAuthenticationQuerier>()
					.Should().Not.Be.Null();
			}
		}

		[SetUp]
		public void Setup()
		{
			builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
		}
	}
}