using Autofac;
using DotNetOpenAuth.OpenId.Provider;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.SSO.Core.IoC
{
	public class SSOAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OpenIdProviderWapper>().As<IOpenIdProviderWapper>().SingleInstance();
			builder.RegisterType<ApplicationAuthenticationType>().As<IIdentityProviderAuthenticationType>().SingleInstance();
			builder.RegisterType<DataSourcesViewModelFactory>().As<IDataSourcesViewModelFactory>().SingleInstance();
			builder.RegisterType<OpenIdProvider>().SingleInstance();
			builder.RegisterType<WindowsAccountProvider>().As<IWindowsAccountProvider>().SingleInstance();
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();
		}
	}
}
