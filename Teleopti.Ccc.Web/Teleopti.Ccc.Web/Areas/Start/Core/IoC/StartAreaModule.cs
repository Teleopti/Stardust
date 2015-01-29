using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

namespace Teleopti.Ccc.Web.Areas.Start.Core.IoC
{
	public class StartAreaModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public StartAreaModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WebLogOn>().As<IWebLogOn>();
			builder.RegisterType<DataSourcesProvider>().As<IDataSourcesProvider>().SingleInstance();
			if (_configuration.Toggle(Toggles.MultiTenancy_WebLogon_17461))
			{
				builder.RegisterType<MultiTenantAuthenticator>().As<IAuthenticator>().SingleInstance();
				builder.RegisterType<LogTenancyLogonAttempt>().As<ILogLogonAttempt>().SingleInstance();
			}
			else
			{
				builder.RegisterType<Authenticator>().As<IAuthenticator>().SingleInstance();
				builder.RegisterType<LogLogonAttempt>().As<ILogLogonAttempt>().SingleInstance();
			}
			builder.RegisterType<LoginAttemptModelFactoryForWeb>().As<ILoginAttemptModelFactory>();
			
			builder.RegisterType<BusinessUnitProvider>().As<IBusinessUnitProvider>();
			builder.RegisterType<TokenIdentityProvider>().As<ITokenIdentityProvider>().SingleInstance();
			builder.RegisterType<LayoutBaseViewModelFactory>().As<ILayoutBaseViewModelFactory>();
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
			builder.RegisterType<FormsAuthenticationWrapper>().As<IFormsAuthentication>();
			builder.RegisterType<AvailableIdentityDataSources>().As<IAvailableIdentityDataSources>().SingleInstance();
			builder.RegisterType<MenuViewModelFactory>().As<IMenuViewModelFactory>();
			builder.RegisterType<SessionSpecificDataStringSerializer>().As<ISessionSpecificDataStringSerializer>().SingleInstance();

			builder.RegisterType<BusinessUnitsViewModelFactory>().As<IBusinessUnitsViewModelFactory>();
			builder.RegisterAssemblyTypes(GetType().Assembly)
				.AssignableTo<IAuthenticationType>()
				.As<IAuthenticationType>()
				.SingleInstance();

			builder.RegisterType<AvailableApplicationIdentityDataSource>().As<IAvailableApplicationTokenDataSource>();
			builder.RegisterType<DataSourcesViewModelFactory>().As<IDataSourcesViewModelFactory>().SingleInstance();
		}
	}
}
