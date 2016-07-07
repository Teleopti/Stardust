using Autofac;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Config;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

namespace Teleopti.Ccc.Web.Areas.Start.Core.IoC
{
	public class StartAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WebLogOn>().As<IWebLogOn>();

			builder.RegisterType<SsoAuthenticator>().As<ISsoAuthenticator>().SingleInstance();
			builder.RegisterType<LogLogonAttempt>().As<ILogLogonAttempt>().SingleInstance();
			builder.RegisterType<LoginAttemptModelFactoryForWeb>().As<ILoginAttemptModelFactory>();

			builder.RegisterType<Authenticator>().As<IIdentityLogon>();
			builder.RegisterType<HttpRequestUserAgent>().As<IHttpRequestUserAgent>();
			
			builder.RegisterType<BusinessUnitProvider>().As<IBusinessUnitProvider>();
			builder.RegisterType<TokenIdentityProvider>().As<ITokenIdentityProvider>().SingleInstance();
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<FormsAuthenticationWrapper>().As<IFormsAuthentication>();
			builder.RegisterType<MenuViewModelFactory>().As<IMenuViewModelFactory>();
			builder.RegisterType<SessionSpecificDataStringSerializer>().As<ISessionSpecificDataStringSerializer>().SingleInstance();

			builder.RegisterType<BusinessUnitsViewModelFactory>().As<IBusinessUnitsViewModelFactory>();
			builder.RegisterType<SharedSettingsFactory>().As<ISharedSettingsFactory>().SingleInstance();
		}
	}
}
