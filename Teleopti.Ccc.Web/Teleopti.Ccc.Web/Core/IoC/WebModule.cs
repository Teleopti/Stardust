using System.Web.Http;
using Autofac;
using Autofac.Configuration;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Anywhere.Core.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.Intraday;
using Teleopti.Ccc.Web.Areas.Mart.Core.IoC;
using Teleopti.Ccc.Web.Areas.Messages.Core.Ioc;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.Options.IoC;
using Teleopti.Ccc.Web.Areas.Outbound.core.IoC;
using Teleopti.Ccc.Web.Areas.Permissions;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reports.IoC;
using Teleopti.Ccc.Web.Areas.Requests.Core.IOC;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.IOC;
using Teleopti.Ccc.Web.Areas.SSO.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.IoC;
using Teleopti.Ccc.Web.Areas.Start.Core.LayoutBase;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Logging;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebModule : Module
	{
		private readonly IIocConfiguration _configuration;
		private readonly HttpConfiguration _httpConfiguration;

		public WebModule(IIocConfiguration configuration, HttpConfiguration httpConfiguration)
		{
			_configuration = configuration;
			_httpConfiguration = httpConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterApiControllers(typeof(WebModule).Assembly).ApplyAspects();
			builder.RegisterControllers(typeof(WebModule).Assembly).ApplyAspects();
			builder.RegisterHubs(typeof(WebModule).Assembly).ApplyAspects();

			builder.RegisterModule<MessageBrokerWebModule>();

			if (_httpConfiguration != null)
				builder.RegisterWebApiFilterProvider(_httpConfiguration);
			builder.RegisterModule(new AutofacWebTypesModule());
			builder.RegisterFilterProvider();

			builder.RegisterModule(new BootstrapperModule(_configuration));

			registerCommonTypes(builder);
			registerPortalTypes(builder);
			registerRequestContextTypes(builder);

			builder.RegisterModule<WebCommonModule>();
			builder.RegisterModule(new MyTimeAreaModule(_configuration));
			builder.RegisterModule<SSOAreaModule>();
			builder.RegisterModule<StartAreaModule>();
			builder.RegisterModule(new AnywhereAreaModule(_configuration));
			
			builder.RegisterModule<ForecastingAreaModule>();
			builder.RegisterType<BasicActionThrottler>().As<IActionThrottler>().SingleInstance();
			builder.RegisterModule<ResourceHandlerModule>();

			builder.RegisterModule(new RuleSetModule(_configuration, false));
			builder.RegisterModule(new AuthenticationCachedModule(_configuration));

			builder.RegisterModule(new RtaAreaModule(_configuration));
			builder.RegisterModule(new MartAreaModule(_configuration));

			builder.RegisterModule<NotificationModule>();
			builder.RegisterType<PersonToRoleAssociation>().SingleInstance();

			builder.RegisterModule(new ConfigurationSettingsReader());
			builder.RegisterModule(new TenantServerModule(_configuration));
			builder.RegisterModule<SeatPlannerAreaModule>();
			builder.RegisterModule<OutboundAreaModule>();
			builder.RegisterModule<Web.Areas.People.Core.IoC.PeopleAreaModule>();
			builder.RegisterModule<TeamScheduleAreaModule>();
			builder.RegisterModule<RequestsAreaModule>();
			builder.RegisterModule<ReportsAreaModule>();
			builder.RegisterModule<OptionsAreaModule>();

			//remove me when #36904 is done!
			builder.RegisterType<TranslatedTexts>().SingleInstance();

			tenantWebSpecificTypes(builder);
		}

		private static void registerRequestContextTypes(ContainerBuilder builder)
		{
			builder.RegisterType<SessionPrincipalFactory>().As<ISessionPrincipalFactory>();
			builder.RegisterType<RequestContextInitializer>().As<IRequestContextInitializer>();
			builder.RegisterType<SessionSpecificCookieDataProvider>().As<ISessionSpecificDataProvider>();
			builder.RegisterType<SessionSpecificForIdentityProviderDataProvider>()
				.As<ISessionSpecificForIdentityProviderDataProvider>();
			builder.RegisterType<SessionAuthenticationModule>().As<ISessionAuthenticationModule>();
			builder.RegisterType<DefaultSessionSpecificCookieDataProviderSettings>()
				.As<ISessionSpecificCookieDataProviderSettings>();
			builder.RegisterType<DefaultSessionSpecificCookieForIdentityProviderDataProviderSettings>()
				.As<ISessionSpecificCookieForIdentityProviderDataProviderSettings>();
			builder.RegisterType<SetThreadCulture>().As<ISetThreadCulture>();

			builder.RegisterType<AreaWithPermissionPathProvider>().As<IAreaWithPermissionPathProvider>();
			builder.RegisterType<AbsenceTypesProvider>().As<IAbsenceTypesProvider>();
			builder.RegisterType<PushMessageProvider>().As<IPushMessageProvider>();
			builder.RegisterType<ReportsProvider>().As<IReportsProvider>();
			builder.RegisterType<ReportNavigationModel>().As<IReportNavigationModel>();
			builder.RegisterType<ReportsNavigationProvider>().As<IReportsNavigationProvider>();
			builder.RegisterType<BadgeProvider>().As<IBadgeProvider>();
			builder.RegisterType<AnalyticsPermissionsUpdater>().As<IAnalyticsPermissionsUpdater>().InstancePerRequest().ApplyAspects();
			builder.RegisterType<PermissionsConverter>().As<IPermissionsConverter>().InstancePerRequest();
			builder.RegisterType<ApplicationFunctionResolver>().As<IApplicationFunctionResolver>().InstancePerRequest();
			builder.RegisterType<CommonReportsFactory>().As<ICommonReportsFactory>().SingleInstance();
		}

		private static void registerPortalTypes(ContainerBuilder builder)
		{
			builder.RegisterType<CultureSpecificViewModelFactory>().As<ICultureSpecificViewModelFactory>();
		}

		private static void registerCommonTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ResourceVersion>().As<IResourceVersion>();
			builder.RegisterType<ErrorMessageProvider>().As<IErrorMessageProvider>();
			builder.RegisterType<Log4NetLogger>().AsSelf();
			builder.RegisterType<LazyLoadingManagerWrapper>().As<ILazyLoadingManager>();
			builder.RegisterType<DefaultScenarioScheduleProvider>().As<IScheduleProvider>();
			builder.RegisterType<DefaultScenarioForStudentAvailabilityScheduleProvider>()
				.As<IStudentAvailabilityProvider>();
			builder.RegisterType<VirtualSchedulePeriodProvider>().As<IVirtualSchedulePeriodProvider>();
			builder.RegisterType<DefaultDateCalculator>().As<IDefaultDateCalculator>();
			builder.RegisterType<UrlHelperProvider>().As<IUrlHelper>().SingleInstance();
			builder.RegisterType<IpAddressResolver>().As<IIpAddressResolver>();
			builder.RegisterType<AuthenticationModule>().As<IAuthenticationModule>().SingleInstance();
			builder.RegisterType<IdentityProviderProvider>().As<IIdentityProviderProvider>().SingleInstance();
			builder.RegisterType<IanaTimeZoneProvider>().As<IIanaTimeZoneProvider>().SingleInstance();
			builder.RegisterType<ThemeSettingProvider>().As<ISettingsPersisterAndProvider<ThemeSetting>>().SingleInstance();

			builder.RegisterType<LicenseCustomerNameProvider>().As<ILicenseCustomerNameProvider>().SingleInstance();
		}

		private static void tenantWebSpecificTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			builder.RegisterType<IdAuthentication>().As<IIdAuthentication>().SingleInstance();
			builder.RegisterType<IdUserQuery>().As<IIdUserQuery>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationEncryption>().As<IDataSourceConfigurationEncryption>().SingleInstance();
			builder.RegisterType<PersonInfoMapper>().As<IPersonInfoMapper>().SingleInstance();
			builder.RegisterType<ChangePersonPassword>().As<IChangePersonPassword>().SingleInstance();
			builder.RegisterType<WebTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<CurrentTenantUser>().As<ICurrentTenantUser>().SingleInstance();
		}
	}

}
