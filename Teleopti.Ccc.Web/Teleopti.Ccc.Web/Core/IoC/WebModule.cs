using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Web.Areas.Forecasting.Core.IoC;
using Teleopti.Ccc.Web.Areas.Gamification.core.IoC;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.Mart.Core.IoC;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
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
using Teleopti.Ccc.Web.Areas.People.Core.IoC;
using Teleopti.Ccc.Web.Areas.Permissions;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reports.IoC;
using Teleopti.Ccc.Web.Areas.Requests.Core.IOC;
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

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebModule : Module
	{
		private static IocConfiguration _configuration;
		private readonly HttpConfiguration _httpConfiguration;

		public WebModule(IocConfiguration configuration, HttpConfiguration httpConfiguration)
		{
			_configuration = configuration;
			_httpConfiguration = httpConfiguration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterApiControllers(typeof(WebModule).Assembly).ApplyAspects();
			builder.RegisterControllers(typeof(WebModule).Assembly).ApplyAspects();
			builder.RegisterHubs(typeof(WebModule).Assembly);

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
			builder.RegisterModule(new LegacyRegistrationsFromAnywhere(_configuration));

			builder.RegisterModule<ForecastingAreaModule>();
			builder.RegisterType<BasicActionThrottler>().As<IActionThrottler>().SingleInstance();
			builder.RegisterModule<ResourceHandlerModule>();

			//overwriting domain registration for IClearScheduleEvents, to keep behavior before 47447480a839. Don't really know why this is needed...
			builder.RegisterType<ClearScheduleEvents>().As<IClearScheduleEvents>().SingleInstance();
			builder.RegisterModule(new AuthenticationCachedModule(_configuration));

			builder.RegisterModule<RtaAreaModule>();
			builder.RegisterModule(new MartAreaModule(_configuration));

			builder.RegisterType<PersonToRoleAssociation>().SingleInstance();
			
			builder.RegisterModule<SeatPlannerAreaModule>();
			builder.RegisterModule(new OutboundAreaModule(true));
			builder.RegisterModule( new Areas.People.Core.IoC.PeopleAreaModule(_configuration));
			
			builder.RegisterModule<TeamScheduleAreaModule>();
			builder.RegisterModule<RequestsAreaModule>();
			builder.RegisterModule<ReportsAreaModule>();
			builder.RegisterModule<OptionsAreaModule>();
			builder.RegisterModule<GamificationAreaModel>();

			//remove me when #36904 is done!
			builder.RegisterType<TranslatedTexts>().SingleInstance();

			tenantWebSpecificTypes(builder);
		}

		private static void registerRequestContextTypes(ContainerBuilder builder)
		{
			builder.RegisterType<SessionPrincipalFactory>().As<ISessionPrincipalFactory>().SingleInstance();
			builder.RegisterType<RequestContextInitializer>().As<IRequestContextInitializer>().SingleInstance();
			builder.RegisterType<SessionSpecificWfmCookieProvider>().As<ISessionSpecificWfmCookieProvider>().SingleInstance();
			builder.RegisterType<SessionSpecificTeleoptiCookieProvider>().As<ISessionSpecificTeleoptiCookieProvider>().SingleInstance();
			builder.RegisterType<SessionAuthenticationModule>().As<ISessionAuthenticationModule>().SingleInstance();
			builder.RegisterType<SessionSpecificCookieSettingsProvider>().SingleInstance();
			builder.RegisterType<SetThreadCulture>().As<ISetThreadCulture>().SingleInstance();

			builder.RegisterType<AreaWithPermissionPathProvider>().As<IAreaWithPermissionPathProvider>();
			builder.RegisterType<AbsenceTypesProvider>().As<IAbsenceTypesProvider>();
			builder.RegisterType<PushMessageProvider>().As<IPushMessageProvider>();
			builder.RegisterType<ReportsProvider>().As<IReportsProvider>();
			builder.RegisterType<ReportsNavigationProvider>().As<IReportsNavigationProvider>();
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
			builder.RegisterType<SystemVersion>().SingleInstance();
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

			builder.RegisterType<UserTextTranslator>().As<IUserTextTranslator>().SingleInstance();
			builder.RegisterType<GroupPageViewModelFactory>().SingleInstance();
			builder.RegisterType<DataTokenManager>().As<IDataTokenManager>();
		}

		private static void tenantWebSpecificTypes(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationAuthentication>().As<IApplicationAuthentication>().SingleInstance();
			builder.RegisterType<IdentityAuthentication>().As<IIdentityAuthentication>().SingleInstance();
			builder.RegisterType<IdAuthentication>().As<IIdAuthentication>().SingleInstance();
			builder.RegisterType<IdUserQuery>().As<IIdUserQuery>().SingleInstance();
			builder.RegisterType<DataSourceConfigurationEncryption>().As<IDataSourceConfigurationEncryption>().SingleInstance();
			builder.RegisterType<PersonInfoMapper>().As<IPersonInfoMapper>().SingleInstance();
			builder.RegisterType<PasswordManager>().As<IPasswordManager>().SingleInstance();
			builder.RegisterType<CustomerDbConnector>().As<ICustomerDbConnector>().SingleInstance();
			builder.RegisterType<NotificationServiceClient>().As<INotificationServiceClient>().SingleInstance();
			builder.RegisterType<WebTenantAuthentication>().As<ITenantAuthentication>().SingleInstance();
			builder.RegisterType<CurrentTenantUser>().As<ICurrentTenantUser>().SingleInstance();
			builder.RegisterType<TenantLogonDataManager>().As<ITenantLogonDataManager>().SingleInstance();
		}
	}
}