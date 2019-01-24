using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class TenantClientModuleTest
	{
		private ContainerBuilder builder;

		[Test]
		public void CanResolveTenantQuerier()
		{
			using (var container = builder.Build())
			{
				container.Resolve<IAuthenticationTenantClient>()
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