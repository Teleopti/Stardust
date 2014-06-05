using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class UnitOfWorkModuleTest
	{
		[Test]
		public void ShouldResolveLicenseActivatorProvider()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<AuthenticationModule>();
			using (var container = builder.Build())
			{
				container.ResolveNamed<ILicenseActivatorProvider>("loggedon").Should().Be.OfType<LicenseActivatorProvider>();
			}
		}
	}
}