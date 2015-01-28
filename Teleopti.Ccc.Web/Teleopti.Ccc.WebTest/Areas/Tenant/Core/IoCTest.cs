using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class IoCTest
	{
		[Test]
		public void CanResolveApplicationAuthentication()
		{
			using (var container = buildContainer())
			{
				container.Resolve<IApplicationAuthentication>()
					.Should().Not.Be.Null();
				container.Resolve<IIdentityAuthentication>()
					.Should().Not.Be.Null();
			}
		}

		private static IContainer buildContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new WebAppModule(new IocConfiguration(new IocArgs(), null)));

			return builder.Build();
		}
	}
}