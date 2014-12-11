using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	public class UnitOfWorkModuleTest
	{
		[Test]
		public void ShouldResolveLicenseActivatorProvider()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(CommonModule.ForTest());
			using (var container = builder.Build())
			{
				container.Resolve<ILicenseActivatorProvider>().Should().Be.OfType<LicenseActivatorProvider>();
			}
		}
	}
}