using System.Configuration;
using Autofac;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Provider;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;

namespace Teleopti.Ccc.Web.Areas.SSO.Core.IoC
{
	public class SSOAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OpenIdProviderWapper>().As<IOpenIdProviderWapper>().SingleInstance();
			builder.RegisterType<ProviderEndpointWrapper>().As<IProviderEndpointWrapper>().SingleInstance();
			var usePersistentCryptoKeys = ConfigurationManager.AppSettings["UsePersistentCryptoKeys"];
			if (!string.IsNullOrEmpty(usePersistentCryptoKeys) && usePersistentCryptoKeys.ToLowerInvariant() == "true")
			{
				builder.RegisterType<SqlProviderApplicationStore>().As<IOpenIdApplicationStore>().SingleInstance();
			}
			builder.RegisterType<OpenIdProvider>().SingleInstance();
			builder.RegisterType<CurrentHttpContext>().As<ICurrentHttpContext>().SingleInstance();
		}
	}
}
