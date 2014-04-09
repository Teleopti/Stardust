using System.Reflection;
using DotNetOpenAuth.OpenId.Provider;
using Teleopti.Ccc.Web.Areas.SSO.Core;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core.IoC
{
	public class SSOAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OpenIdProviderWapper>().As<IOpenIdProviderWapper>().SingleInstance();
			builder.RegisterType<OpenIdProvider>().SingleInstance();
			builder.RegisterType<WindowsAccountProvider>().As<IWindowsAccountProvider>().SingleInstance();
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();
		}
	}
}
